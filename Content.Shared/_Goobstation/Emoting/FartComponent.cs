using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
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
