using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Magic.Components;

namespace Content.Goobstation.Shared.Wizard.Chuuni;

public sealed class GetSpellInvocationEvent(MagicSchool school, EntityUid performer) : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.EYES;

    public MagicSchool School = school;

    public EntityUid Performer = performer;

    public DamageSpecifier ToHeal = new();

    public LocId? Invocation;
}
