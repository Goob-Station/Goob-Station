using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Works like NestedTileShape, but also has serialization,
/// so you can just type id: in prototypes and it will work.
/// </summary>
public sealed partial class ProtoIdMegafaunaAction : MegafaunaActionSelector
{
    public const string IdDataFieldTag = "id";

    [DataField(IdDataFieldTag, required: true)]
    public ProtoId<MegafaunaActionPrototype> Id;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var action = args.PrototypeMan.Index(Id).Action;
        action.CopyFrom(this);
        return action.Invoke(args);
    }
}
