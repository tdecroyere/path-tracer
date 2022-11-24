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

DllExport void* PT_CreateImageSurface(void* window, int width, int height)
{
    auto nativeWindow = (NativeWindow*)window;
    auto nativeImageSurface = new NativeImageSurface();

    nativeImageSurface->WindowHandle = nativeWindow->WindowHandle;
    nativeImageSurface->Width = width;
    nativeImageSurface->Height = height;

    auto bitmapInfo = BITMAPINFO();

    bitmapInfo = {};
    bitmapInfo.bmiHeader.biSize = sizeof(bitmapInfo.bmiHeader);
	bitmapInfo.bmiHeader.biWidth = width;
	bitmapInfo.bmiHeader.biHeight = -height;
	bitmapInfo.bmiHeader.biPlanes = 1;
	bitmapInfo.bmiHeader.biBitCount = 32;
	bitmapInfo.bmiHeader.biCompression = BI_RGB;

    nativeImageSurface->BitmapInfo = bitmapInfo;

    return nativeImageSurface;
}

DllExport NativeImageSurfaceInfo PT_GetImageSurfaceInfo(void* imageSurface)
{
    auto result = NativeImageSurfaceInfo();
    result.GreenShift = 8;
    result.BlueShift = 0;
    result.RedShift = 16;
    result.AlphaShift = 24;

    return result;
}

DllExport void PT_UpdateImageSurface(void* imageSurface, unsigned char* data)
{
    /*auto nativeImageSurface = (NativeImageSurface*)imageSurface;
    HDC deviceContext = GetDC(nativeImageSurface->WindowHandle);

	RECT windowRectangle;
	GetClientRect(nativeImageSurface->WindowHandle, &windowRectangle);

	int windowWidth = windowRectangle.right - windowRectangle.left;
	int windowHeight = windowRectangle.bottom - windowRectangle.top;

	StretchDIBits(deviceContext,
		0, 0, windowWidth, windowHeight,
		0, 0, nativeImageSurface->Width, nativeImageSurface->Height,
		data,
		&nativeImageSurface->BitmapInfo,
		DIB_RGB_COLORS, SRCCOPY);

	ReleaseDC(nativeImageSurface->WindowHandle, deviceContext);*/
}

DllExport void* PT_CreatePanel(void* window)
{
    return nullptr;
}

DllExport void* PT_CreateButton(void* parent, unsigned char* text)
{
    return nullptr;
}