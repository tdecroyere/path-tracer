#include "WindowsCommon.h"
#include "../Platform.h"

DllExport void PT_GetInputState(void* application, NativeInputState* inputState)
{
    printf("InputState\n");
}