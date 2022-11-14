#include "WindowsCommon.h"
#include "NativeUIServiceUtils.h"
#include "../Platform.h"

struct NativeApplication
{
    HINSTANCE ApplicationInstance;
};

struct NativeWindow
{
    HWND WindowHandle;
    int Width;
    int Height;
};

struct NativeImageSurface
{
    HWND WindowHandle;
    BITMAPINFO BitmapInfo;
    int Width;
    int Height;
};

DllExport void* PT_CreateApplication(unsigned char* applicationName)
{
    auto application = new NativeApplication();

    application->ApplicationInstance = (HINSTANCE)GetModuleHandle(nullptr);

	WNDCLASS windowClass {};
	windowClass.style = CS_HREDRAW | CS_VREDRAW;
	windowClass.lpfnWndProc = Win32WindowCallBack;
	windowClass.hInstance = application->ApplicationInstance;
	windowClass.lpszClassName = L"PathTracerWindowClass";
	windowClass.hCursor = LoadCursor(NULL, IDC_ARROW);

    RegisterClass(&windowClass);

    return application;
}

DllExport NativeAppStatus PT_ProcessSystemMessages(void* application)
{
    auto result = NativeAppStatus();
    result.IsRunning = 1;

    bool gameRunning = true;
    MSG message;

	while (PeekMessage(&message, nullptr, 0, 0, PM_REMOVE))
	{
        if (message.message == WM_QUIT)
        {
            result.IsRunning = 0;
            return result;
        }

        TranslateMessage(&message);
        DispatchMessage(&message);
    }

    return result;
}

DllExport void* PT_CreateWindow(void* application, unsigned char* title, int width, int height, NativeWindowState windowState)
{
    auto nativeApplication = (NativeApplication*)application;

    // Create the window
    HWND window = CreateWindowEx(0,
        L"PathTracerWindowClass",
        ConvertUtf8ToWString(title).c_str(),
        WS_OVERLAPPEDWINDOW | WS_VISIBLE,
        0,
        0,
        width,
        height,
        0,
        0,
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

    SetWindowPos(window, nullptr, x, y, width, height, 0);

    if (windowState == NativeWindowState::Maximized)
    {
        ShowWindow(window, SW_MAXIMIZE);
    }

    auto nativeWindow = new NativeWindow();
    nativeWindow->WindowHandle = window;
    nativeWindow->Width = width;
    nativeWindow->Height = height;

    return nativeWindow;
}

DllExport NativeWindowSize PT_GetWindowRenderSize(void* window)
{
    auto nativeWindow = (NativeWindow*)window;

    RECT windowRectangle;
	GetClientRect(nativeWindow->WindowHandle, &windowRectangle);

    nativeWindow->Width = windowRectangle.right - windowRectangle.left;
    nativeWindow->Height = windowRectangle.bottom - windowRectangle.top;
    
    auto result = NativeWindowSize();
    result.Width = nativeWindow->Width;
    result.Height = nativeWindow->Height;

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
    auto nativeImageSurface = (NativeImageSurface*)imageSurface;
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

	ReleaseDC(nativeImageSurface->WindowHandle, deviceContext);
}