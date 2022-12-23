#include "WindowsCommon.h"
#include "../Platform.h"
#include "Libs/Win32DarkMode/DarkMode.h"
#include "NativeUIServiceUtils.h"
#include "NativeApplicationService.h"

struct NativeWindow
{
    HWND WindowHandle;
    int Width;
    int Height;
    float UIScale;
};

struct NativeImageSurface
{
    HWND WindowHandle;
    BITMAPINFO BitmapInfo;
    int Width;
    int Height;
};

DllExport void* PT_CreateWindow(void* application, unsigned char* title, int width, int height, NativeWindowState windowState)
{
    InitCommonControls();
    auto nativeApplication = (NativeApplication*)application;

    // Create the window
    auto window = CreateWindowEx(
        WS_EX_DLGMODALFRAME,
        L"PathTracerWindowClass",
        ConvertUtf8ToWString(title).c_str(),
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        width,
        height,
        nullptr,
        nullptr,
        nativeApplication->ApplicationInstance,
        0);

    HMODULE shcoreLibrary = LoadLibrary(L"shcore.dll");

    SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
  
    auto mainScreenDpi = GetDpiForWindow(window);
    auto mainScreenScaling = static_cast<float>(mainScreenDpi) / 96.0f;

    RECT clientRectangle;
    clientRectangle.left = 0;
    clientRectangle.top = 0;
    clientRectangle.right = static_cast<LONG>(width * mainScreenScaling);
    clientRectangle.bottom = static_cast<LONG>(height * mainScreenScaling);

    AdjustWindowRectExForDpi(&clientRectangle, WS_OVERLAPPEDWINDOW, false, 0, mainScreenDpi);

    width = clientRectangle.right - clientRectangle.left;
    height = clientRectangle.bottom - clientRectangle.top;

    // Compute the position of the window to center it 
    RECT desktopRectangle;
    GetClientRect(GetDesktopWindow(), &desktopRectangle);
    int x = (desktopRectangle.right / 2) - (width / 2);
    int y = (desktopRectangle.bottom / 2) - (height / 2);

    // Dark mode
    // TODO: Don't include the full library for this
    InitDarkMode();
    BOOL value = TRUE;
    
    if (ShouldAppUseDarkMode())
    {
        DwmSetWindowAttribute(window, DWMWA_USE_IMMERSIVE_DARK_MODE, &value, sizeof(value));
    }

    SetWindowPos(window, nullptr, x, y, width, height, 0);
    ShowWindow(window, SW_NORMAL);

    if (windowState == NativeWindowState::Maximized)
    {
        ShowWindow(window, SW_MAXIMIZE);
    }

    auto nativeWindow = new NativeWindow();
    nativeWindow->WindowHandle = window;
    nativeWindow->Width = width;
    nativeWindow->Height = height;
    nativeWindow->UIScale = mainScreenScaling;

    return nativeWindow;
}

DllExport void* PT_GetWindowSystemHandle(void* window)
{
    auto nativeWindow = (NativeWindow*)window;
    return nativeWindow->WindowHandle;
}

DllExport NativeWindowSize PT_GetWindowRenderSize(void* window)
{
    auto nativeWindow = (NativeWindow*)window;

    RECT windowRectangle;
	GetClientRect(nativeWindow->WindowHandle, &windowRectangle);

    auto mainScreenDpi = GetDpiForWindow(nativeWindow->WindowHandle);
    auto mainScreenScaling = static_cast<float>(mainScreenDpi) / 96.0f;

    nativeWindow->Width = windowRectangle.right - windowRectangle.left;
    nativeWindow->Height = windowRectangle.bottom - windowRectangle.top;
    nativeWindow->UIScale = mainScreenScaling;

    auto result = NativeWindowSize();
    result.Width = nativeWindow->Width;
    result.Height = nativeWindow->Height;
    result.UIScale = nativeWindow->UIScale;

    return result;
}

DllExport void PT_SetWindowTitle(void* window, unsigned char* title)
{
    auto nativeWindow = (NativeWindow*)window;
    SetWindowText(nativeWindow->WindowHandle, ConvertUtf8ToWString(title).c_str());
}