using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Lavaland.Damage;

/// <summary>
/// This handles modifying melee Damage of an entity
/// </summary>
public sealed class ModifyMeleeDamageSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModifyMeleeDamageComponent,GetUserMeleeDamageEvent>(OnGetMeleeDamage);
    }

    private void OnGetMeleeDamage(Entity<ModifyMeleeDamageComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        args.Damage *= ent.Comp.Modifier;
    }
}
