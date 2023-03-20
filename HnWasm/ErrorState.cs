namespace HnWasm;

internal class ErrorState
{
    internal Action? OnChanged;

    public bool ResetUrl { get; private set; }
    internal bool IsError { get; private set; }

    internal void SetError(bool isError, bool resetUrl = false)
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