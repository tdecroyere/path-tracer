namespace PathTracer;

public class FrameTimer
{
    private readonly Stopwatch _frameStopwatch;
    private readonly Stopwatch _framesPerSecondsStopwatch;
    private int _framesPerSecondsCounter;

    public FrameTimer()
    {
        _frameStopwatch = new Stopwatch();
        _frameStopwatch.Start();

        _framesPerSecondsStopwatch = new Stopwatch();
        _framesPerSecondsStopwatch.Start();
    }

    public int FramesPerSeconds
    {
        get;
        private set;
    }

    public float DeltaTime
    {
        get;
        private set;
    }

    public void Update()
    {
        _framesPerSecondsCounter++;
        DeltaTime = _frameStopwatch.ElapsedMilliseconds * 0.001f;
        _frameStopwatch.Restart();

        if (_framesPerSecondsStopwatch.ElapsedMilliseconds >= 1000)
        {
            FramesPerSeconds = _framesPerSecondsCounter;
            _framesPerSecondsCounter = 0;
            _framesPerSecondsStopwatch.Restart();
        }
    }
}