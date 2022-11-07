param ($outputDirectory, $Configuration)

function RegisterVisualStudioEnvironment
{
    $registeredVisualStudioVersion = Get-Content -Path Env:VisualStudioVersion -ErrorAction SilentlyContinue

    if (-not($registeredVisualStudioVersion -eq "16.0") -And -not($registeredVisualStudioVersion -eq "17.0"))
    {
        Write-Output "[93mRegistering Visual Studio Environment...[0m"

        # TODO: Do something better here
        $vsPath = ""
        $vs2019ComPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"
        $vs2019ProfPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Auxiliary\Build\vcvars64.bat"
        $vs2019EntPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\VC\Auxiliary\Build\vcvars64.bat"
        $vs2019PreviewPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Preview\VC\Auxiliary\Build\vcvars64.bat"
        $vs2022PreviewPath = "C:\Program Files\Microsoft Visual Studio\2022\Preview\VC\Auxiliary\Build\vcvars64.bat"
        
        if (Test-Path -Path $vs2019ComPath)
        {
            $vsPath = $vs2019ComPath
        }

        if (Test-Path -Path $vs2019ProfPath)
        {
            $vsPath = $vs2019ProfPath
        }

        if (Test-Path -Path $vs2019EntPath)
        {
            $vsPath = $vs2019EntPath
        }

        if (Test-Path -Path $vs2019PreviewPath)
        {
            $vsPath = $vs2019PreviewPath
        }

        if (Test-Path -Path $vs2022PreviewPath)
        {
            $vsPath = $vs2022PreviewPath
        }

        $batchCommand = "`"$vsPath`" > nul & set"

        cmd /c $batchCommand | Foreach-Object {
            $p, $v = $_.split('=')
            Set-Item -path env:$p -value $v
        }
    }
}


function PreCompileHeader {
    Push-Location "obj"
    if (-Not(Test-Path -Path "WindowsCommon.pch")) {
        Write-Output "[93mCompiling Windows Pre-compiled header...[0m"

        if ($Configuration -eq "Debug") {
            cl.exe /c /nologo /DUNICODE /D_UNICODE /DDEBUG /std:c++17 /Zi /EHsc /Yc /FpWindowsCommon.pch "../WindowsCommon.cpp"
        } else {
            cl.exe /c /nologo /DUNICODE /D_UNICODE /std:c++17 /O2 /Zi /EHsc /Yc /FpWindowsCommon.pch "../WindowsCommon.cpp"
        }

        if(-Not $?) {
            Pop-Location
            ShowErrorMessage
            Exit 1
        }
    }
    Pop-Location
}

function CompileWindowsHost {
    Push-Location obj

    Write-Output "[93mCompiling Windows Library...[0m"

    if ($Configuration -eq "Debug") {
        cl.exe /c /nologo /DDEBUG /std:c++17 /DUNICODE /D_UNICODE /Zi /diagnostics:caret /EHsc /Yu"WindowsCommon.h" /FpWindowsCommon.PCH /TP /Tp"..\UnityBuild.cpp"
    } else {
        cl.exe /c /nologo /std:c++17 /O2 /DUNICODE /D_UNICODE /Zi /diagnostics:caret /EHsc /Yu"WindowsCommon.h" /FpWindowsCommon.PCH /TP /Tp"..\UnityBuild.cpp"
    }

    if (-Not $?)
    {
        Pop-Location
        ShowErrorMessage
        Exit 1
    }

    Pop-Location
}

function LinkWindowsHost
{
    Push-Location obj
    Write-Output "[93mLinking Windows Library...[0m"

    if ($Configuration -eq "Debug") {
        link.exe "UnityBuild.obj" "WindowsCommon.obj" /OUT:"PathTracer.Platform.Native.dll" /PDB:"PathTracer.Platform.Native.pdb" /DLL /DEBUG /MAP /OPT:ref /INCREMENTAL:NO /WINMD:NO /NOLOGO uuid.lib libcmt.lib libvcruntimed.lib libucrtd.lib kernel32.lib user32.lib gdi32.lib ole32.lib advapi32.lib Winmm.lib
    } else {
        link.exe "UnityBuild.obj" "WindowsCommon.obj" /OUT:"PathTracer.Platform.Native.dll" /PDB:"PathTracer.Platform.Native.pdb" /DLL /DEBUG /MAP /OPT:ref /INCREMENTAL:NO /WINMD:NO /NOLOGO uuid.lib libcmt.lib libvcruntimed.lib libucrtd.lib kernel32.lib user32.lib gdi32.lib ole32.lib advapi32.lib Winmm.lib
    }

    if (-Not $?)
    {
        Pop-Location
        ShowErrorMessage
        Exit 1
    }

    Pop-Location
}

try
{
    $outputDirectory = Resolve-Path $outputDirectory

    Write-Output "[93mCompiling Windows Platform Library...[0m"
    Push-Location ./Platforms/Windows

    mkdir obj | Out-Null

    RegisterVisualStudioEnvironment
    PreCompileHeader
    CompileWindowsHost
    LinkWindowsHost
   
    if (-Not $?) {
        Pop-Location
        Exit 1
    }
    
    Write-Output "Copying files...$outputDirectory"
    Copy-Item obj/*.dll $outputDirectory -Recurse -Force
    Copy-Item obj/*.pdb $outputDirectory -Recurse -Force
}

finally
{
    Pop-Location
}