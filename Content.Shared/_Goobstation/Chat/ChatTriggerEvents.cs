namespace Content.Shared._Goobstation.Chat;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseChatTriggerEvent : EntityEventArgs
{
    public EntityUid Performer;

    public string Message;
}
