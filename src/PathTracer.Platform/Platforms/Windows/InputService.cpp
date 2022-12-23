#include "WindowsCommon.h"
#include "../Platform.h"

#define GET_X_LPARAM(lp)    ((int)(short)LOWORD(lp))
#define GET_Y_LPARAM(lp)    ((int)(short)HIWORD(lp))

auto applicationInputQueues = std::map<void*, std::vector<WindowsEvent>>();

void NativeInputProcessKeyboardEvent(void* application, WindowsEvent event) 
{
    if (applicationInputQueues.count(application) == 0) 
    {
        applicationInputQueues[application] = std::vector<WindowsEvent>();
    }

    applicationInputQueues[application].push_back(event);
}

void ProcessKey(InputObject *inputObjects, InputObjectKey key, int windowsKeyCode, WindowsEvent event)
{
    if (event.WParam == windowsKeyCode)
    {
        inputObjects[key].Value = (event.Message == WM_KEYDOWN) ? 1.0f : 0.0f;

        if (event.Message == WM_KEYDOWN)
        {
            if ((event.LParam & 0xFF) == 1)
            {
                inputObjects[key].Repeatcount += 1;
            }
        }
        else
        {
            inputObjects[key].Repeatcount = 0;
        }
    }
}

DllExport void PT_UpdateInputState(void* application, InputState* inputState)
{
    // TODO: Bind correct application
    InputObject* inputObjects = (InputObject*)inputState->InputObjectPointer;

    if (applicationInputQueues.count(nullptr) == 0) 
    {
        return;
    }

    auto events = applicationInputQueues[nullptr];

    for (auto i = 0; i < events.size(); i++)
    {
        auto event = events[i];

        if (event.Message == WM_KEYDOWN || event.Message == WM_KEYUP)
        {
            ProcessKey(inputObjects, InputObjectKey::KeyA, 'A', event);
            ProcessKey(inputObjects, InputObjectKey::KeyB, 'B', event);
            ProcessKey(inputObjects, InputObjectKey::KeyC, 'C', event);
            ProcessKey(inputObjects, InputObjectKey::KeyD, 'D', event);
            ProcessKey(inputObjects, InputObjectKey::KeyE, 'E', event);
            ProcessKey(inputObjects, InputObjectKey::KeyF, 'F', event);
            ProcessKey(inputObjects, InputObjectKey::KeyG, 'G', event);
            ProcessKey(inputObjects, InputObjectKey::KeyH, 'H', event);
            ProcessKey(inputObjects, InputObjectKey::KeyI, 'I', event);
            ProcessKey(inputObjects, InputObjectKey::KeyJ, 'J', event);
            ProcessKey(inputObjects, InputObjectKey::KeyK, 'K', event);
            ProcessKey(inputObjects, InputObjectKey::KeyL, 'L', event);
            ProcessKey(inputObjects, InputObjectKey::KeyM, 'M', event);
            ProcessKey(inputObjects, InputObjectKey::KeyN, 'N', event);
            ProcessKey(inputObjects, InputObjectKey::KeyO, 'O', event);
            ProcessKey(inputObjects, InputObjectKey::KeyP, 'P', event);
            ProcessKey(inputObjects, InputObjectKey::KeyQ, 'Q', event);
            ProcessKey(inputObjects, InputObjectKey::KeyR, 'R', event);
            ProcessKey(inputObjects, InputObjectKey::KeyS, 'S', event);
            ProcessKey(inputObjects, InputObjectKey::KeyT, 'T', event);
            ProcessKey(inputObjects, InputObjectKey::KeyU, 'U', event);
            ProcessKey(inputObjects, InputObjectKey::KeyV, 'V', event);
            ProcessKey(inputObjects, InputObjectKey::KeyW, 'W', event);
            ProcessKey(inputObjects, InputObjectKey::KeyX, 'X', event);
            ProcessKey(inputObjects, InputObjectKey::KeyY, 'Y', event);
            ProcessKey(inputObjects, InputObjectKey::KeyZ, 'Z', event);
            
            ProcessKey(inputObjects, InputObjectKey::Up, VK_UP, event);
            ProcessKey(inputObjects, InputObjectKey::Down, VK_DOWN, event);
            ProcessKey(inputObjects, InputObjectKey::Left, VK_LEFT, event);
            ProcessKey(inputObjects, InputObjectKey::Right, VK_RIGHT, event);
        }
        else if (event.Message == WM_LBUTTONDOWN || event.Message == WM_LBUTTONUP)
        {
            inputObjects[InputObjectKey::MouseLeftButton].Value = (event.Message == WM_LBUTTONDOWN) ? 1.0f : 0.0f;
        }
        else if (event.Message == WM_MOUSEMOVE)
        {
            inputObjects[InputObjectKey::MouseAxisX].Value = (float)GET_X_LPARAM(event.LParam);
            inputObjects[InputObjectKey::MouseAxisY].Value = (float)GET_Y_LPARAM(event.LParam);
        }
    }
    
    applicationInputQueues[nullptr].clear();
}