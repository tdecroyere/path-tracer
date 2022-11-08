
public readonly record struct NativeApplicationStatus
{
    public NativeApplicationStatus()
    {
        IsRunning = 1;
        IsActive = 1;
    }

    public int IsRunning { get; init; }
    public int IsActive { get; init; }
}