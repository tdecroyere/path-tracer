namespace PathTracer;

public static class Utils
{
    public static int ConvertBytesToMegaBytes(long value)
    {
        return (int)(value / 1024 / 1024);
    }
}