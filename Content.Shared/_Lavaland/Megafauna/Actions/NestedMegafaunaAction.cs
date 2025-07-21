using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Action that references a ProtoId containing other megafauna actions.
/// </summary>
public sealed partial class NestedMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public ProtoId<MegafaunaActionPrototype> Id;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        return args.PrototypeMan.Index(Id).Action.Invoke(args, IsSequence, Counter);
    }
}
