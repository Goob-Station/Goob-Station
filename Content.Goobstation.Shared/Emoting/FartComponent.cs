using Content.Shared.Chat.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Emoting;

// use as a template
//[Serializable, NetSerializable, DataDefinition] public sealed partial class AnimationNameEmoteEvent : EntityEventArgs { }

[RegisterComponent, NetworkedComponent]
public sealed partial class FartComponent : Component
{
    [DataField] public ProtoId<EmotePrototype>? Emote;
    [DataField] public bool FartTimeout = false;
    [DataField] public bool FartInhale = false;
    [DataField] public bool SuperFarted = false;
    [DataField] public float MolesAmmoniaPerFart = 5f;

    /// <summary>
    ///     Path to the sound when you get bible smited
    /// </summary>
    [DataField]
    [Access(Other = AccessPermissions.ReadWriteExecute)]
    public SoundSpecifier BibleSmiteSnd = new SoundPathSpecifier("/Audio/_Goobstation/Effects/thunder_clap.ogg");
}

[Serializable, NetSerializable]
public sealed partial class FartComponentState : ComponentState
{
    public ProtoId<EmotePrototype>? Emote;
    public bool FartTimeout;
    public bool FartInhale;
    public bool SuperFarted;

    public FartComponentState(ProtoId<EmotePrototype>? emote, bool fartTimeout, bool fartInhale, bool superFarted)
    {
        Emote = emote;
        FartTimeout = fartTimeout;
        FartInhale = fartInhale;
        SuperFarted = superFarted;
    }
}

/// <summary>
///     Triggers after a fart 🦍💨
/// </summary>
public sealed class PostFartEvent : EntityEventArgs
{
    public readonly EntityUid Uid;
    public readonly bool SuperFart;
    public PostFartEvent(EntityUid uid, bool IsSuperFart = false)
    {
        Uid = uid;
        SuperFart = IsSuperFart;
    }
}

[Serializable, NetSerializable]
public sealed class BibleFartSmiteEvent(NetEntity uid) : EntityEventArgs
{
    public NetEntity Bible = uid;
}
