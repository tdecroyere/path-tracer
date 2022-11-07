#include "WindowsCommon.h"

LRESULT CALLBACK Win32WindowCallBack(HWND window, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_ACTIVATE:
	{
		//isAppActive = !(wParam == WA_INACTIVE);
		break;
	}
	case WM_KEYDOWN:
	{
		/*if (globalInputService != nullptr)
		{
			globalInputService->UpdateRawInputKeyboardState(WM_KEYDOWN, wParam);
		}*/
		break;
	}
	case WM_SYSKEYUP:
		/*if (!isDirect3d && wParam == VK_RETURN)
		{
			if ((HIWORD(lParam) & KF_ALTDOWN))
			{
				Win32SwitchScreenMode(window);
			}
		}*/
		break;
	case WM_KEYUP:
	{
		switch (wParam)
		{
		case VK_ESCAPE:
			::PostQuitMessage(0);
			break;
		}

		/*if (globalInputService != nullptr)
		{
			globalInputService->UpdateRawInputKeyboardState(WM_KEYUP, wParam);
		}*/
		break;
	}
	case WM_SIZE:
	{
		//doChangeSize = true;
		// TODO: Handle minimized state
		break;
	}
    case WM_DPICHANGED:
    {
        RECT* const prcNewWindow = (RECT*)lParam;
        SetWindowPos(window,
            NULL,
            prcNewWindow ->left,
            prcNewWindow ->top,
            prcNewWindow->right - prcNewWindow->left,
            prcNewWindow->bottom - prcNewWindow->top,
            SWP_NOZORDER | SWP_NOACTIVATE);

        break;
    }
	case WM_CLOSE:
	case WM_DESTROY:
	{
		PostQuitMessage(0);
		break;
	}
	default:
		return DefWindowProcA(window, message, wParam, lParam);
	}

	return 0;
}