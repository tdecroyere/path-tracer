try
{
    Push-Location ./tests/PathTracer.Core.UnitTests

    $env:CoverletOutputFormat="lcov"
    $env:CoverletOutput="../../coverage/lcov-PathTracer.UnitTests.info"

    dotnet test --nologo -v m /p:CollectCoverage=true
}

finally
{
    Pop-Location
}
