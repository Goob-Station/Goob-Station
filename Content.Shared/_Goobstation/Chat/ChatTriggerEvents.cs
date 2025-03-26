using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.Chat;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseChatTriggerEvent : EntityEventArgs
{
    public EntityUid Performer;

    public string Message;
}

public sealed partial class BecomeTheSwapEvent : BaseChatTriggerEvent
{
    [DataField]
    public int ServerCurrencyCost = 250;

    [DataField]
    public bool IpcOnly = true;

    [DataField]
    public SoundSpecifier? TransformSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/bamf.ogg");
}
