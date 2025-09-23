using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Revenant;

/// <summary>
/// The target immediately collapses and begins to take a huge amount of brute damage over time
/// as their bones crack and their body implodes. The victim explodes into gibs once this damage becomes lethal,
/// but the process is interrupted if they are removed from your line of sight or you move (or are moved).
/// </summary>
public sealed class RevenantCrushSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevenantCrushComponent, RevenantCrushEvent>(OnRevenantCrush);
        SubscribeLocalEvent<RevenantCrushComponent, RevenantCrushDoAfterEvent>(OnRevenantCrushDoAfter);
    }

    private void OnRevenantCrush(Entity<RevenantCrushComponent> ent, ref RevenantCrushEvent args)
    {
        _stunSystem.KnockdownOrStun(args.Target, ent.Comp.KnockdownDuration, true);
        _damageableSystem.TryChangeDamage(args.Target, ent.Comp.InitialDamage, true);
        // TODO: popup here

        var doAftersArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.AbilityDuration,
            new RevenantCrushDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            // technically, in order for them to be removed from our line of sight, they need to move...
            BreakOnMove = true,
            MovementThreshold = ent.Comp.MovementThreshold,
        };

        _doAfter.TryStartDoAfter(doAftersArgs);
        args.Handled = true;
    }

    private void OnRevenantCrushDoAfter(Entity<RevenantCrushComponent> ent, ref RevenantCrushDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        // TODO: popup here
        _body.GibBody(args.Target.Value, splatModifier: 5f);

        args.Handled = true;
    }
}
