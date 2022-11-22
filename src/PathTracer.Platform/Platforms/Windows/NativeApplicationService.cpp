#include "WindowsCommon.h"
#include "../Platform.h"
#include "NativeUIServiceUtils.h"
#include "NativeApplicationService.h"

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