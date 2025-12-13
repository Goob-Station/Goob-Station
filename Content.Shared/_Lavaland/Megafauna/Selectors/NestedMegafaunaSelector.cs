using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Action that references a ProtoId containing other megafauna actions.
/// </summary>
public sealed partial class NestedMegafaunaSelector : MegafaunaSelector
{
    [DataField(required: true)]
    public ProtoId<MegafaunaSelectorPrototype> Id;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        return args.PrototypeMan.Index(Id).Selector.Invoke(args);
    }
}
