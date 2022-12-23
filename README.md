# Path Tracer

[![codecov](https://codecov.io/gh/tdecroyere/path-tracer/branch/main/graph/badge.svg?token=6VHKU17MD8)](https://codecov.io/gh/tdecroyere/path-tracer)

Based on the awesome [The Cherno YouTube Ray Tracing serie](https://www.youtube.com/playlist?list=PLlrATfBNZ98edc5GshdBtREv5asFW3yXl).

## Current Features:

- Runs on Windows and MacOS with .NET 7.
- Platform independant layer written in C++ for Windows and Swift for MacOS.
- Use ImGui for UI.
- Use Veldrid for graphics for now. (Will have native Vulkan, Direct3D and Metal in a later phase)

## Usage:

From the project root directory run the following command:

    dotnet run --project src/PathTracer -c Release

## Renders:
20/10/2022:
![Output from 20/10/2022](TestData/Archive/20221020.png)