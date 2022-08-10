namespace LittleToyZenjectify;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

internal class ServiceDescriptor
{
    public INamedTypeSymbol ServiceType;
    public string TargetInstallerNameName;
    public InjectionMethod InjectionMethod;
    public bool IsLazyLoading;
    public bool BindInterfacesAndSelf;
    public string Suffix;
    public bool FromInstance => InjectionMethod == InjectionMethod.MonoClassWithSceneObjInstance || InjectionMethod == InjectionMethod.MonoClassWithAssetInstance;
    public IEnumerable<IMethodSymbol> CandidateConstructors => ServiceType.GetMembers().OfType<IMethodSymbol>().Where(_ => _.HasAttribute("Inject"));
}
