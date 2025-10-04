namespace Content.Goobstation.Server.Shizophrenia;

[DataDefinition]
public sealed partial class RemoveHallucinationsEvent : EntityEventArgs
{
    [DataField]
    public string Id = "";
}
