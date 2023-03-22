namespace HnWasm;

internal class ErrorState
{
    public Action? OnChanged;

    public bool ResetUrl { get; private set; }
    public bool IsError { get; private set; }

    public void SetError(bool isError, bool resetUrl = false)
    {
        if (IsError == isError)
        {
            return;
        }
        ResetUrl = resetUrl;
        IsError = isError;
        OnChanged?.Invoke();
    }
}