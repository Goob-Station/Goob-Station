namespace Content.Server._Goobstation.Chemistry.HyposprayBlockNonMobInjection;

/// <summary>
/// For some reason if you set HyposprayComponent onlyAffectsMobs to true it would be able to draw from containers
/// even if injectOnly is also true. I don't want to modify HypospraySystem, so I made this component.
/// </summary>
[RegisterComponent]
public sealed partial class HyposprayBlockNonMobInjectionComponent : Component
{
}
