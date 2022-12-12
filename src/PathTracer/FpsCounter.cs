namespace PathTracer;

public class FpsCounter
{
    private readonly Stopwatch _framesPerSecondsStopWatch;
    private int _framesPerSecondsCounter;

    public FpsCounter()
    {
        _framesPerSecondsStopWatch = new Stopwatch();
        _framesPerSecondsStopWatch.Start();
    }

    public int FramesPerSeconds
    {
        get;
        private set;
    }

    public void Update()
    {
        _framesPerSecondsCounter++;

        if (_framesPerSecondsStopWatch.ElapsedMilliseconds >= 1000)
        {
            FramesPerSeconds = _framesPerSecondsCounter;
            _framesPerSecondsCounter = 0;
            _framesPerSecondsStopWatch.Restart();
        }
    }
}