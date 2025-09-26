using System.Diagnostics;
using Content.Goobstation.Shared.Damage;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server;

/// <summary>
/// This handles increses the Melee damage depending not how mutch its bleeding
/// </summary>
public sealed class BleedingIncreaseMeleeDamageSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BleedingIncreaseMeleeDamageComponent,GetUserMeleeDamageEvent>(OnGetMeleeDamage);
    }

    private void OnGetMeleeDamage(Entity<BleedingIncreaseMeleeDamageComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        if (!TryComp<BloodstreamComponent>(ent.Owner, out var blood))
            return;

        var ratio = blood.BleedAmount/blood.MaxBleedAmount;

        var damageModifier = ent.Comp.Modifier * ratio;

        args.Damage *= Math.Max(1, damageModifier); // make sure its not negative

    }
}


