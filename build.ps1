# Version for hex2dfu is set in version.json 
dotnet pack .\source\nanoFramework.Tools.Hex2Dfu.csproj --configuration Release --output .\tool --runtime win-x86 --verbosity detailed -p:Platform=x86