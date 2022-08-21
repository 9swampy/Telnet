#Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
#http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd
#http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd
#http://schemas.microsoft.com/packaging/2016/06/nuspec.xsd

if (-not (Test-Path -Path Nuget.exe -PathType Leaf)) {
  Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile Nuget.exe
}

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
