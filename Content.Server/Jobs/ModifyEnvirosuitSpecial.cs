using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Content.Shared.SelfExtinguisher;
using JetBrains.Annotations;

namespace Content.Server.Jobs;

[UsedImplicitly]
public sealed partial class ModifyEnvirosuitSpecial : JobSpecial
{
    // <summary>
    //   The new charges of the envirosuit's self-extinguisher.
    // </summary>
    [DataField(required: true)]
    public int Charges { get; private set; }

    [ValidatePrototypeId<SpeciesPrototype>]
    private const string Species = "Plasmaman";

    private const string Slot = "jumpsuit";

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        if (!entMan.TryGetComponent<HumanoidAppearanceComponent>(mob, out var appearance) ||
            appearance.Species != Species ||
            !entMan.System<InventorySystem>().TryGetSlotEntity(mob, Slot, out var jumpsuit) ||
            jumpsuit is not { } envirosuit ||
            !entMan.TryGetComponent<SelfExtinguisherComponent>(envirosuit, out var selfExtinguisher))
            return;

        entMan.System<SharedSelfExtinguisherSystem>().SetCharges(envirosuit, Charges, Charges, selfExtinguisher);
    }
}
