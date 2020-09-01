#Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
#http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd
#http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd
#http://schemas.microsoft.com/packaging/2016/06/nuspec.xsd
Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile Nuget.exe
.\nuget pack Telnet.nuspec
