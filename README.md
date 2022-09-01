LittleToy Source generator
===========================

On any OS
```
dotnet build -c Release
cp LittleToySourceGenerator/bin/Release/netstandard2.0/LittleToySourceGenerator.dll ../crowdcityonline/Assets/Plugins/basegame/
```

Debugging in case of something really wierd.
```
dotnet build
cp LittleToySourceGenerator/bin/Debug/netstandard2.0/LittleToySourceGenerator.dll ../crowdcityonline/Assets/Plugins/basegame/
```

## Docs generator
On any OS
```
dotnet build -c Release
cp LittleToyDocumentor/bin/Release/netstandard2.0/LittleToyDocumentor.dll ../crowdcityonline/Assets/Plugins/basegame/
``````

Debugging in case of something really wierd.
```
dotnet build
cp LittleToyDocumentor/bin/Debug/netstandard2.0/LittleToyDocumentor.dll ../crowdcityonline/Assets/Plugins/basegame/
```

## Zenject generator
On any OS
```
dotnet build -c Release
cp LittleToyZenjectify/bin/Release/netstandard2.0/LittleToyZenjectify.dll ../crowdcityonline/Assets/Plugins/basegame/
```

## Testing automation

First commit changes to the branch and send PR to this repo.
Afterwards.

```
$CurrentBranch=$(git branch --show-current)
$LastMessage=$(git show -s --format=%s)
pushd ../crowdcityonline
git pull
git submodule update
pushd Assets/Plugins/basegame/
git add *.dll
git switch -c $CurrentBranch
git commit -m "$LastMessage"
git push --set-upstream origin $CurrentBranch
popd
git switch -c $CurrentBranch
git add Assets/Plugins/basegame
git commit -m "$LastMessage"
git push --set-upstream origin $CurrentBranch
popd
```