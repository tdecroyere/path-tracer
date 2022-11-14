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

struct NativeWindowSize
{
    int Width;
    int Height;
};

enum NativeInputObjectType
{
    Digital,
    Analog,
    Relative
};

struct NativeInputObject
{
    enum NativeInputObjectType ObjectType;
    float Value;
    float PreviousValue;
};

struct NativeKeyboard
{
    struct NativeInputObject KeyA;
    struct NativeInputObject KeyD;
    struct NativeInputObject KeyQ;
    struct NativeInputObject KeyS;
    struct NativeInputObject KeyZ;
    struct NativeInputObject ArrowUp;
    struct NativeInputObject ArrowDown;
    struct NativeInputObject ArrowLeft;
    struct NativeInputObject ArrowRight;
};

struct NativeInputState
{
    struct NativeKeyboard Keyboard;
};