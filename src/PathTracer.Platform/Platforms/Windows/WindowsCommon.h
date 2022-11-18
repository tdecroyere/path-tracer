#pragma once
#include <stdint.h>
#include <sys/stat.h>

#include <locale>
#include <codecvt>
#include <string>

#include <map>
#include <vector>
#include <stack>
#include <assert.h>

#include <ShellScalingAPI.h>
#include <Windows.h>
#include <Uxtheme.h>

/*#include <unknwn.h>

#include <winrt/base.h>
#include <winrt/Windows.UI.Composition.h>

#include <winrt/Microsoft.UI.h>
#include <winrt/Microsoft.UI.Composition.h>
#include <winrt/Microsoft.UI.Interop.h>
#include <winrt/Microsoft.UI.Windowing.h>
*/
#define AssertIfFailed(result) assert(!FAILED(result))

#define DllExport extern "C" __declspec(dllexport)