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

void ProcessKey(NativeInputObject *inputObjects, NativeInputObjectKey key, int windowsKeyCode, WindowsEvent event)
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

DllExport void PT_UpdateInputState(void* application, NativeInputState* inputState)
{
    // TODO: Bind correct application
    NativeInputObject* inputObjects = (NativeInputObject*)inputState->InputObjectPointer;

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
            ProcessKey(inputObjects, NativeInputObjectKey::KeyA, 'A', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyB, 'B', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyC, 'C', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyD, 'D', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyE, 'E', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyF, 'F', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyG, 'G', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyH, 'H', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyI, 'I', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyJ, 'J', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyK, 'K', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyL, 'L', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyM, 'M', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyN, 'N', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyO, 'O', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyP, 'P', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyQ, 'Q', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyR, 'R', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyS, 'S', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyT, 'T', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyU, 'U', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyV, 'V', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyW, 'W', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyX, 'X', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyY, 'Y', event);
            ProcessKey(inputObjects, NativeInputObjectKey::KeyZ, 'Z', event);

        }
        else if (event.Message == WM_LBUTTONDOWN || event.Message == WM_LBUTTONUP)
        {
            inputObjects[NativeInputObjectKey::MouseLeftButton].Value = (event.Message == WM_LBUTTONDOWN) ? 1.0f : 0.0f;
        }
        else if (event.Message == WM_MOUSEMOVE)
        {
            inputObjects[NativeInputObjectKey::MouseAxisX].Value = GET_X_LPARAM(event.LParam);
            inputObjects[NativeInputObjectKey::MouseAxisY].Value = GET_Y_LPARAM(event.LParam);
        }
    }
    
    applicationInputQueues[nullptr].clear();
}