namespace Content.Goobstation.Shared.ExplodeServer;

public sealed class ExplodeServerEvent : EntityEventArgs
{
    public bool IsExploding { get; }
    public bool Exploded { get; }

    public ExplodeServerEvent(bool isExploding, bool exploded)
    {
        IsExploding = isExploding;
        Exploded = exploded;
    }
}
