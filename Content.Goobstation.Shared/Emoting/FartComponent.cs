// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Emoting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class FartComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextFart;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromMinutes(1);

    [DataField, AutoNetworkedField]
    public bool Inhaled;

    [DataField, AutoNetworkedField]
    public bool Buttless;

    [DataField]
    public Gas Gas = Gas.Ammonia;

    [DataField]
    public float Moles = 5f;

    [DataField]
    public float SuperFartMolesMultiplier = 2f;

    [DataField]
    public EntProtoId ButtPrototype = "Butt";

    [DataField]
    public float ButtThrowDistance = 2f;

    [DataField]
    public float ShakeRange = 10f;

    [DataField]
    public float SuperFartShakeIntensity = 0.75f;

    [DataField]
    public float BibleSmiteShakeIntensity = 1.5f;

    [DataField]
    public float BibleSmiteRange = 0.4f;

    [DataField]
    public SoundSpecifier FartSound = new SoundCollectionSpecifier("Fart");

    [DataField]
    public SoundSpecifier InhaleSound = new SoundCollectionSpecifier("FartInhale");

    [DataField]
    public SoundSpecifier SuperFartSound = new SoundCollectionSpecifier("SuperFart");

    [DataField]
    public SoundSpecifier BibleSmiteSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/thunder_clap.ogg");
}

[ByRefEvent]
public readonly record struct PostFartEvent(bool SuperFart);

[Serializable, NetSerializable]
public sealed class BibleFartSmiteEvent(NetEntity bible) : EntityEventArgs
{
    public readonly NetEntity Bible = bible;
}
