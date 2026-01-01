using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Obelisk.Species.Components;

[RegisterComponent]
public sealed partial class HydrakinComponent : Component
{
    /// <summary>
    /// What percentage of temperature should be removed each ability use.
    /// </summary>
    /// <example>
    /// CoolOffCoefficient = 0.5, T = 100, the temperature after the ability would be 50
    /// </example>
    [DataField]
    public float CoolOffCoefficient = 0.1f;

    [DataField]
    public EntProtoId<InstantActionComponent> CoolOffActionId = "ActionHydrakinCoolOff";

    [DataField]
    public SoundSpecifier? CoolOffSound = new SoundCollectionSpecifier("HydrakinFlap");

    [DataField]
    public EntityUid? CoolOffAction;
}
