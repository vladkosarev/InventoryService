@echo off
cls
packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe Install FAKE -OutputDirectory packages -ExcludeVersion
packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe Install ConfigJson -OutputDirectory packages -ExcludeVersion
packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe install xunit.runner.console -OutputDirectory packages\FAKE -ExcludeVersion -Version 2.1.0
packages\FAKE\tools\Fake.exe build-server.fsx buildType=%1 nugetDeployPath=%2
#packages\FAKE\tools\Fake.exe build-service.fsx buildType=%1 nugetDeployPath=%2
#packages\FAKE\tools\Fake.exe build-server-ui.fsx buildType=%1 nugetDeployPath=%2
pause