using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorWebComponent : Component
{
    /// <summary>
    /// How long someone stays ensnared once they step onto spider webs.
    /// </summary>
    [DataField]
    public TimeSpan SnareTime = TimeSpan.FromSeconds(5);


    [DataField]
    public SoundSpecifier CaughtSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");
}
