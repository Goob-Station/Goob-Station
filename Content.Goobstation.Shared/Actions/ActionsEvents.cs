using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Actions;

[Serializable, NetSerializable]
public sealed class ActionRemovedFromUIControllerMessage : EntityEventArgs
{
    public NetEntity Action;

    public ActionRemovedFromUIControllerMessage(NetEntity action)
    {
        Action = action;
    }
}

/// <summary>
///     This one gets raised on the action after registering the message.
/// </summary>
[Serializable, NetSerializable]
public sealed class ActionRemovedFromUIControllerEvent : EntityEventArgs;
