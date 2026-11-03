using Content.Shared.Damage;

namespace Content.Shared.Armor;

public abstract partial class SharedArmorSystem
{
    public void SetModifiers(Entity<ArmorComponent> ent, DamageModifierSet modifiers)
    {
        ent.Comp.Modifiers = modifiers;
        Dirty(ent);
    }
}
