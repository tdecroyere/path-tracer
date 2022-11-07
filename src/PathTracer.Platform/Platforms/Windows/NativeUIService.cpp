#include "WindowsCommon.h"
#include "NativeUIServiceUtils.h"
#include "../NativeUIService.h"

struct NativeApplication
{
    HINSTANCE ApplicationInstance;
};

struct NativeWindow
{
    HWND WindowHandle;
};

struct NativeImageSurface
{
    HWND WindowHandle;
    BITMAPINFO* BitmapInfo;
    int Width;
    int Height;
};

DllExport void* CreateApplication(unsigned char* applicationName)
{
    printf("Init Windows\n");

    auto application = new NativeApplication();

    application->ApplicationInstance = (HINSTANCE)GetModuleHandle(nullptr);

	WNDCLASS windowClass {};
	windowClass.style = CS_HREDRAW | CS_VREDRAW;
	windowClass.lpfnWndProc = Win32WindowCallBack;
	windowClass.hInstance = application->ApplicationInstance;
	windowClass.lpszClassName = L"PathTracerWindowClass";
	windowClass.hCursor = LoadCursor(NULL, IDC_ARROW);

	if (RegisterClass(&windowClass))
	{
        HMODULE shcoreLibrary = LoadLibrary(L"shcore.dll");

		//SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

        //this->mainScreenDpi = GetDpiForWindow(GetDesktopWindow());
        //this->mainScreenScaling = static_cast<float>(this->mainScreenDpi) / 96.0f;
    }

    return application;
}

DllExport NativeAppStatus ProcessSystemMessages(void* application)
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

DllExport void* CreateNativeWindow(void* application, unsigned char* title, int width, int height, NativeWindowState windowState)
{
    auto nativeApplication = (NativeApplication*)application;

    printf("Init Window\n");

    /*RECT clientRectangle;
    clientRectangle.left = 0;
    clientRectangle.top = 0;
    clientRectangle.right = static_cast<LONG>(width * this->mainScreenScaling);
    clientRectangle.bottom = static_cast<LONG>(height * this->mainScreenScaling);

    AdjustWindowRectExForDpi(&clientRectangle, WS_OVERLAPPEDWINDOW, false, 0, this->mainScreenDpi);*/

    //width = clientRectangle.right - clientRectangle.left;
    //height = clientRectangle.bottom - clientRectangle.top;

    // Compute the position of the window to center it 
    RECT desktopRectangle;
    GetClientRect(GetDesktopWindow(), &desktopRectangle);
    int x = (desktopRectangle.right / 2) - (width / 2);
    int y = (desktopRectangle.bottom / 2) - (height / 2);

    // Create the window
    HWND window = CreateWindowEx(0,
        L"PathTracerWindowClass",
        L"Teeeest",
        WS_OVERLAPPEDWINDOW | WS_VISIBLE,
        x,
        y,
        width,
        height,
        0,
        0,
        nativeApplication->ApplicationInstance,
        0);

    if (windowState == NativeWindowState::Maximized)
    {
        ShowWindow(window, SW_MAXIMIZE);
    }

    auto nativeWindow = new NativeWindow();
    nativeWindow->WindowHandle = window;

    return nativeWindow;
}

DllExport void* CreateImageSurface(void* window, int width, int height)
{
    auto nativeWindow = (NativeWindow*)window;
    auto nativeImageSurface = new NativeImageSurface();

    nativeImageSurface->WindowHandle = nativeWindow->WindowHandle;
    nativeImageSurface->Width = width;
    nativeImageSurface->Height = height;

    auto bitmapInfo = (LPBITMAPINFO)malloc(sizeof(BITMAPINFO) + sizeof(RGBQUAD) * 3);

    *bitmapInfo = {};

    bitmapInfo->bmiHeader.biSize = sizeof(bitmapInfo->bmiHeader);
	bitmapInfo->bmiHeader.biWidth = width;
	bitmapInfo->bmiHeader.biHeight = -height;
	bitmapInfo->bmiHeader.biPlanes = 1;
	bitmapInfo->bmiHeader.biBitCount = 32;
	bitmapInfo->bmiHeader.biCompression = BI_RGB;
 
	/*bitmapInfo->bmiHeader.biCompression = BI_BITFIELDS;

    DWORD *bmiColors = (DWORD*)bitmapInfo->bmiColors;
    
    bmiColors[0] = 0x000000ff;
    bmiColors[1] = 0x0000ff00;
    bmiColors[2] = 0x00ff0000;*/

    nativeImageSurface->BitmapInfo = bitmapInfo;

    return nativeImageSurface;
}

DllExport NativeImageSurfaceInfo GetImageSurfaceInfo(void* imageSurface)
{
    auto result = NativeImageSurfaceInfo();
    result.GreenShift = 8;
    result.BlueShift = 0;
    result.RedShift = 16;
    result.AlphaShift = 24;

    return result;
}

DllExport void UpdateImageSurface(void* imageSurface, unsigned char* data)
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
		nativeImageSurface->BitmapInfo,
		DIB_RGB_COLORS, SRCCOPY);

	ReleaseDC(nativeImageSurface->WindowHandle, deviceContext);
}