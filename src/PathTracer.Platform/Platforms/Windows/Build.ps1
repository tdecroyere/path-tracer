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


function RestoreNugetPackages
{
    $nuGetExe = ".\nuget.exe"
    $packagesFile = "..\..\..\packages.config"
    $packagesDirectory = ".\packages"
    $includeDirectory = ".\inc"

    Push-Location $generatedFilesFolder

    if (-not(Test-Path($nuGetExe))) 
    {
        Write-Output "Downloading nuget.exe..."
        Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $nuGetExe
    }

    if (-not(Test-Path($packagesDirectory))) 
    {
        & $nuGetExe "restore" $packagesFile "-PackagesDirectory" $packagesDirectory

        if (-not $?) 
        {
            Write-Output "[91mError: Nuget restore has failed![0m"
        }
    }

    $mdFolder = "C:\perso\path-tracer\src\PathTracer.Platform\Platforms\Windows\obj\Debug\Generated Files\packages\Microsoft.WindowsAppSDK.1.2.221109.1\lib\uap10.0"

    if (-not(Test-Path($includeDirectory))) 
    {
        Write-Output "[93mGenerating C++/WinRT 2.0 include files...[0m"
        $winrtProgram = (Get-ChildItem -Path $packagesDirectory -Filter "Microsoft.Windows.CppWinRT*" -Recurse -Directory).Fullname + "\bin\cppwinrt.exe"
        & $winrtProgram "-input" "$mdFolder" "-output" $includeDirectory

        if (-not $?) 
        {
            Write-Output "[91mError: Winrt has failed![0m"
        }
    }

    Pop-Location
}

function ShowErrorMessage
{
    Write-Output "[91mError: Build has failed![0m"
}

function PreCompileHeader {
    Push-Location $objFolder
    if (-Not(Test-Path -Path "WindowsCommon.pch")) {
        Write-Output "[93mCompiling Windows Pre-compiled header...[0m"

        if ($Configuration -eq "Debug") {
            cl.exe /c /nologo /DUNICODE /D_UNICODE /DDEBUG /std:c++17 /Zi /EHsc /Yc /FpWindowsCommon.pch "..\..\WindowsCommon.cpp"
        } else {
            cl.exe /c /nologo /DUNICODE /D_UNICODE /std:c++17 /O2 /Zi /EHsc /Yc /FpWindowsCommon.pch "..\..\WindowsCommon.cpp"
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
    Push-Location $objFolder

    Write-Output "[93mCompiling Windows Library...[0m"

    if ($Configuration -eq "Debug") {
        cl.exe /c /nologo /DDEBUG /std:c++17 /DUNICODE /D_UNICODE /Zi /diagnostics:caret /EHsc /Yu"WindowsCommon.h" /FpWindowsCommon.PCH /TP /Tp"..\..\UnityBuild.cpp"
    } else {
        cl.exe /c /nologo /std:c++17 /O2 /DUNICODE /D_UNICODE /Zi /diagnostics:caret /EHsc /Yu"WindowsCommon.h" /FpWindowsCommon.PCH /TP /Tp"..\..\UnityBuild.cpp"
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
    Push-Location $objFolder
    Write-Output "[93mLinking Windows Library...[0m"

    if ($Configuration -eq "Debug") {
        link.exe "UnityBuild.obj" "WindowsCommon.obj" /OUT:"PathTracer.Platform.Native.dll" /PDB:"PathTracer.Platform.Native.pdb" /DLL /DEBUG /MAP /OPT:ref /INCREMENTAL:NO /WINMD:NO /NOLOGO WindowsApp.lib Dwmapi.lib uuid.lib libcmt.lib libvcruntimed.lib libucrtd.lib kernel32.lib user32.lib gdi32.lib ole32.lib advapi32.lib Winmm.lib
    } else {
        link.exe "UnityBuild.obj" "WindowsCommon.obj" /OUT:"PathTracer.Platform.Native.dll" /PDB:"PathTracer.Platform.Native.pdb" /DLL /DEBUG /MAP /OPT:ref /INCREMENTAL:NO /WINMD:NO /NOLOGO WindowsApp.lib Dwmapi.lib uuid.lib libcmt.lib libvcruntimed.lib libucrtd.lib kernel32.lib user32.lib gdi32.lib ole32.lib advapi32.lib Winmm.lib
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

    $objFolder = "./obj/$Configuration"

    if (-not(Test-Path $objFolder)) 
    {
        New-Item -Path $objFolder -ItemType "directory" | Out-Null
    }

    $generatedFilesFolder = "$objFolder\Generated Files\"

    if (-not(Test-Path $generatedFilesFolder)) 
    {
        New-Item -Path $generatedFilesFolder -ItemType "directory" | Out-Null
    }

    RegisterVisualStudioEnvironment
    #RestoreNugetPackages
    PreCompileHeader
    CompileWindowsHost
    LinkWindowsHost
   
    if (-Not $?) {
        Pop-Location
        Exit 1
    }
    
    Write-Output "Copying files...$outputDirectory"
    Copy-Item $objFolder/*.dll $outputDirectory -Recurse -Force
    Copy-Item $objFolder/*.pdb $outputDirectory -Recurse -Force
}

finally
{
    Pop-Location
}