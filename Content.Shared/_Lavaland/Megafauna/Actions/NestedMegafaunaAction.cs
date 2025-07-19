using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Shape that covers a ProtoId containing other shapes.
/// </summary>
public sealed partial class NestedMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public ProtoId<MegafaunaActionPrototype> Id;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        return args.PrototypeMan.Index(Id).Action.Invoke(args, Counter);
    }
}
