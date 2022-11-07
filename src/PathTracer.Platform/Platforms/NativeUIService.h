struct NativeAppStatus
{
    int IsRunning;
    int IsActive;
};

enum NativeWindowState
{
    Normal,
    Maximized
};

struct NativeImageSurfaceInfo
{
    int RedShift;
    int GreenShift;
    int BlueShift;
    int AlphaShift;
};