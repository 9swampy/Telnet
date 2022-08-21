#Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
#http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd
#http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd
#http://schemas.microsoft.com/packaging/2016/06/nuspec.xsd

if (-not (Test-Path -Path Nuget.exe -PathType Leaf)) {
  Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile Nuget.exe
}

.\nuget restore
dotnet restore

msbuild -restore

msbuild PrimS.Telnet.sln /p:Configuration=Debug /p:Platform="Any CPU"

#with mstests
vstest.console.exe **\bin\**\PrimS.Telnet.Sync.CiTests.dll
vstest.console.exe PrimS.Telnet.48.CiTests\bin\Debug\**\PrimS.Telnet.48.CiTests.dll
dotnet vstest PrimS.Telnet.CiTests\bin\Debug\**\PrimS.Telnet.CiTests.dll
dotnet vstest **\bin\Debug\**\PrimS.Telnet.NetStandard.CiTests.dll

dotnet tool install --global GitVersion.Tool
$str = dotnet-gitversion /updateprojectfiles | out-string
$json = ConvertFrom-Json $str
$semVer = $json.SemVer
$fullSemVer = $json.FullSemVer
$nuGetVersionV2 = $json.NuGetVersionV2

Write-Host $str
Write-Host $semVer
Write-Host $fullSemVer
Write-Host $nuGetVersionV2

msbuild PrimS.Telnet.sln /p:Configuration=Release /p:Platform="Any CPU"

.\nuget pack Telnet.nuspec -Version $nuGetVersionV2


