param(
    [string]$BaseUrl = "https://localhost:7188",
    [string]$OutDir = "visual-check",
    [string]$Edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
)

# Mobile visual smoke test for the Blazor app: opens a few routes in headless Edge,
# waits for real rendered content, checks for horizontal overflow and Blazor errors,
# and writes screenshots to the output folder for quick inspection.
$ErrorActionPreference = "Stop"
$repo = Resolve-Path (Join-Path $PSScriptRoot "..")
$out = Join-Path $repo $OutDir
$profile = Join-Path $out "edge-profile"
$port = 9460
$server = $null
$edgeProcess = $null

function Test-Server {
    try {
        & curl.exe -k -s -I "$BaseUrl/" --max-time 5 | Out-Null
        return $LASTEXITCODE -eq 0
    }
    catch {
        return $false
    }
}

function Read-CdpMessage {
    $buffer = New-Object byte[] 4194304
    $builder = [Text.StringBuilder]::new()
    do {
        $result = $script:ws.ReceiveAsync([ArraySegment[byte]]::new($buffer), [Threading.CancellationToken]::None).GetAwaiter().GetResult()
        [void]$builder.Append([Text.Encoding]::UTF8.GetString($buffer, 0, $result.Count))
    } while (-not $result.EndOfMessage)
    return $builder.ToString()
}

function Send-Cdp($method, $params = $null) {
    $script:id++
    $msg = @{ id = $script:id; method = $method }
    if ($null -ne $params) {
        $msg.params = $params
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes(($msg | ConvertTo-Json -Depth 20 -Compress))
    $script:ws.SendAsync([ArraySegment[byte]]::new($bytes), [System.Net.WebSockets.WebSocketMessageType]::Text, $true, [Threading.CancellationToken]::None).GetAwaiter().GetResult()
    while ($true) {
        $obj = (Read-CdpMessage) | ConvertFrom-Json
        if ($obj.id -eq $script:id) {
            return $obj
        }
    }
}

try {
    Remove-Item -LiteralPath $out -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $out,$profile | Out-Null

    if (-not (Test-Server)) {
        $serverOut = Join-Path $out "server.out.log"
        $serverErr = Join-Path $out "server.err.log"
        $server = Start-Process dotnet -ArgumentList @("run","--project","HnWasm/HnWasm.csproj","--launch-profile","https","--no-build") -WorkingDirectory $repo -RedirectStandardOutput $serverOut -RedirectStandardError $serverErr -WindowStyle Hidden -PassThru
        for ($i = 0; $i -lt 30 -and -not (Test-Server); $i++) {
            Start-Sleep -Seconds 1
        }
        if (-not (Test-Server)) {
            throw "App did not start at $BaseUrl"
        }
    }

    $edgeProcess = Start-Process $Edge -ArgumentList @("--headless=new","--disable-gpu","--ignore-certificate-errors","--no-first-run","--no-default-browser-check","--remote-debugging-port=$port","--user-data-dir=$profile","about:blank") -WindowStyle Hidden -PassThru
    Start-Sleep -Seconds 2

    $targets = ((& curl.exe -s "http://127.0.0.1:$port/json/list") -join "`n") | ConvertFrom-Json
    $page = $targets | Where-Object { $_.type -eq "page" -and $_.url -eq "about:blank" } | Select-Object -First 1
    if ($null -eq $page) {
        throw "No Edge page target found"
    }

    $script:ws = [System.Net.WebSockets.ClientWebSocket]::new()
    $script:ws.ConnectAsync([Uri]([string]$page.webSocketDebuggerUrl), [Threading.CancellationToken]::None).GetAwaiter().GetResult() | Out-Null
    $script:id = 0

    Send-Cdp "Page.enable" | Out-Null
    Send-Cdp "Runtime.enable" | Out-Null
    Send-Cdp "Emulation.setDeviceMetricsOverride" @{ width = 375; height = 812; deviceScaleFactor = 1; mobile = $true } | Out-Null

    $checks = @(
        @{ Name = "index"; Url = "$BaseUrl/"; Ready = "document.querySelectorAll('.outer .info').length >= 10" },
        @{ Name = "page-2"; Url = "$BaseUrl/2"; Ready = "document.querySelectorAll('.outer .info').length >= 10" },
        @{ Name = "story-48732953"; Url = "$BaseUrl/story/48732953"; Ready = "document.body.innerText.includes('light-weight-logger') && document.querySelectorAll('pre').length >= 4" },
        @{ Name = "story-48734373"; Url = "$BaseUrl/story/48734373"; Ready = "document.body.innerText.includes('Claude Code is steganographically marking requests') && document.querySelectorAll('.heading').length >= 50" }
    )

    $results = @()
    foreach ($check in $checks) {
        Send-Cdp "Page.navigate" @{ url = $check.Url } | Out-Null
        $ready = $false
        for ($i = 0; $i -lt 80 -and -not $ready; $i++) {
            Start-Sleep -Milliseconds 500
            $expr = "($($check.Ready)) && getComputedStyle(document.querySelector('#blazor-error-ui')).display !== 'block'"
            $ready = [bool](Send-Cdp "Runtime.evaluate" @{ expression = $expr; returnByValue = $true }).result.result.value
        }
        if (-not $ready) {
            Send-Cdp "Page.navigate" @{ url = $check.Url } | Out-Null
            for ($i = 0; $i -lt 80 -and -not $ready; $i++) {
                Start-Sleep -Milliseconds 500
                $expr = "($($check.Ready)) && getComputedStyle(document.querySelector('#blazor-error-ui')).display !== 'block'"
                $ready = [bool](Send-Cdp "Runtime.evaluate" @{ expression = $expr; returnByValue = $true }).result.result.value
            }
        }

        $metricsExpr = @'
(() => {
  const main = document.querySelector('main');
  const badPlaceholders = Array.from(document.querySelectorAll('.heading'))
    .filter(heading => heading.innerText.trim().startsWith('ago ['))
    .length;
  return {
    clientWidth: document.documentElement.clientWidth,
    scrollWidth: document.documentElement.scrollWidth,
    bodyScrollWidth: document.body.scrollWidth,
    mainScrollWidth: main?.scrollWidth,
    errorDisplay: getComputedStyle(document.querySelector('#blazor-error-ui')).display,
    badPlaceholders
  };
})()
'@
        $metrics = (Send-Cdp "Runtime.evaluate" @{ expression = $metricsExpr; returnByValue = $true }).result.result.value
        $screenshot = Send-Cdp "Page.captureScreenshot" @{ format = "png"; captureBeyondViewport = $false }
        [IO.File]::WriteAllBytes((Join-Path $out ($check.Name + ".png")), [Convert]::FromBase64String([string]$screenshot.result.data))

        $passed = $ready -and $metrics.errorDisplay -eq "none" -and $metrics.badPlaceholders -eq 0 -and $metrics.scrollWidth -le $metrics.clientWidth -and $metrics.bodyScrollWidth -le $metrics.clientWidth -and $metrics.mainScrollWidth -le $metrics.clientWidth
        $results += [pscustomobject]@{
            Name = $check.Name
            Passed = $passed
            Ready = $ready
            ClientWidth = $metrics.clientWidth
            ScrollWidth = $metrics.scrollWidth
            BodyScrollWidth = $metrics.bodyScrollWidth
            MainScrollWidth = $metrics.mainScrollWidth
            ErrorDisplay = $metrics.errorDisplay
            BadPlaceholders = $metrics.badPlaceholders
        }
    }

    $results | Format-Table -AutoSize
    if ($results.Passed -contains $false) {
        throw "Visual check failed"
    }
}
finally {
    if ($script:ws) {
        $script:ws.Dispose()
    }
    if ($edgeProcess -and -not $edgeProcess.HasExited) {
        Stop-Process -Id $edgeProcess.Id -Force
    }
    if ($server -and -not $server.HasExited) {
        Stop-Process -Id $server.Id -Force
    }
}
