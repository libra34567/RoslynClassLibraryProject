LittleToy Source generator
===========================

On any OS
```
dotnet build -c Release
cp LittleToySourceGenerator/bin/Release/netstandard2.0/LittleToySourceGenerator.dll ../RoslynTestProject/Assets/
```

Debugging in case of something really wierd.
```
dotnet build
cp LittleToySourceGenerator/bin/Debug/netstandard2.0/LittleToySourceGenerator.dll ../RoslynTestProject/Assets/
```

## Docs generator
On any OS
```
dotnet build -c Release
cp LittleToyDocumentor/bin/Release/netstandard2.0/LittleToyDocumentor.dll ../RoslynTestProject/Assets/
```