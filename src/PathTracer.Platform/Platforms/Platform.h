#pragma once

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
    float UIScale;
};

struct NativeInputObject
{
    float Value;
    float PreviousValue;
    short Repeatcount;
    short PreviousRepeatCount;
};

struct NativeInputState
{
    void *InputObjectPointer;
    int InputObjectCount;
};

enum NativeInputObjectKey
{
    KeyA = 0,
    KeyB = 1,
    KeyC = 2,
    KeyD = 3,
    KeyE = 4,
    KeyF = 5,
    KeyG = 6,
    KeyH = 7,
    KeyI = 8,
    KeyJ = 9,
    KeyK = 10,
    KeyL = 11,
    KeyM = 12,
    KeyN = 13,
    KeyO = 14,
    KeyP = 15,
    KeyQ = 16,
    KeyR = 17,
    KeyS = 18,
    KeyT = 19,
    KeyU = 20,
    KeyV = 21,
    KeyW = 22,
    KeyX = 23,
    KeyY = 24,
    KeyZ = 25,

    Shift = 26,
    Control = 27,
    Menu = 28,
    Pause = 29,
    Capital = 30,
    KeyEscape = 31,
    Space = 32,
    End = 33,
    Home = 34,
    Left = 35,
    Up = 36,
    Down = 37,
    Select = 38,
    Print = 39,
    Insert = 40,
    Delete = 41,
    Back = 42,
    Tab = 43,
    Clear = 44,
    Return = 45,
    LeftSystemButton = 46,
    RightSystemButton = 47,
    Numpad0 = 48,
    Numpad1 = 49,
    Numpad2 = 50,
    Numpad3 = 51,
    Numpad4 = 52,
    Numpad5 = 53,
    Numpad6 = 54,
    Numpad7 = 55,
    Numpad8 = 56,
    Numpad9 = 57,
    Multiply = 58,
    Add = 59,
    Separator = 60,
    Subtract = 61,
    Decimal = 62,
    Divide = 63,
    F1 = 64,
    F2 = 65,
    F3 = 66,
    F4 = 67,
    F5 = 68,
    F6 = 69,
    F7 = 70,
    F8 = 71,
    F9 = 72,
    F10 = 73,
    F11 = 74,
    F12 = 75,

    MouseAxisX = 76,
    MouseAxisY = 77,
    MouseLeftButton = 78,
    MaxValue = 79
};