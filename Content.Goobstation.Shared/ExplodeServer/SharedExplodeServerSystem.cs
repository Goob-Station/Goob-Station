namespace Content.Goobstation.Shared.ExplodeServer;

public sealed class ExplodeServerEvent : EntityEventArgs
{
    public bool IsExploding { get; }

    public ExplodeServerEvent(bool isExploding)
    {
        IsExploding = isExploding;
    }
}
