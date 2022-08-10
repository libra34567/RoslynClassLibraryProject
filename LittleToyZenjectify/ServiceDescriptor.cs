namespace LittleToyZenjectify;

using Microsoft.CodeAnalysis;

internal class ServiceDescriptor
{
    public INamedTypeSymbol ServiceType;
    public string TargetInstallerNameName;
    public InjectionMethod InjectionMethod;
    public bool IsLazyLoading;
    public bool BindInterfacesAndSelf;
    public string Suffix;
    public bool FromInstance => InjectionMethod == InjectionMethod.MonoClassWithSceneObjInstance || InjectionMethod == InjectionMethod.MonoClassWithAssetInstance;
}
