using Content.Goobstation.Common.MisandryBox;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MisandryBox;

public sealed partial class AddComponentSpecial : MarkingSpecial
{
    [DataField(required: true)]
    public ComponentRegistry Components { get; private set; } = new();

    /// <summary>
    /// If this is true then existing components will be removed and replaced with these ones.
    /// </summary>
    [DataField]
    public bool RemoveExisting = true;

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        entMan.AddComponents(mob, Components, removeExisting: RemoveExisting);
    }

    /// <remarks>This completely removes any components markings have added. If you are replacing existing comps - YOU'RE FUCKED</remarks>
    public override void AfterUnequip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        entMan.RemoveComponents(mob, Components);
    }
}
