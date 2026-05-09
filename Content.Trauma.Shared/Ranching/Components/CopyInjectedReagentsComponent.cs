namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Will cause any entity with this component to copy the first reagent injected into it and adds SolutionRegeneration to the blood stream with that reagent
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CopyInjectedReagentsComponent : Component
{
    [DataField]
    public string Solution = "glasschicken";
}
