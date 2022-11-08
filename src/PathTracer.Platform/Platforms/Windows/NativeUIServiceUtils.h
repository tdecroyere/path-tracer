#include "WindowsCommon.h"

#undef CreateWindow

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
		return DefWindowProc(window, message, wParam, lParam);
	}

	return 0;
}

std::wstring ConvertUtf8ToWString(unsigned char* source)
{
    int stringLength = std::string((char*)source).length();
    std::wstring destination;
    destination.resize(stringLength + 1);
    MultiByteToWideChar(CP_UTF8, 0, (char*)source, -1, (wchar_t*)destination.c_str(), stringLength + 1);

	return destination;
}
