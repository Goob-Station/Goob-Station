using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ModSuits.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModCoreComponent : Component;

[RegisterComponent, NetworkedComponent]
[Access(typeof(ModCoreSystem))]
public sealed partial class ModCoreRefuelableComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Fuel = new();

    [DataField]
    public float ChargePerFuel = 200f;

    [DataField]
    public SoundSpecifier RefuelSound = new SoundPathSpecifier("/Audio/Effects/refill.ogg");
}
