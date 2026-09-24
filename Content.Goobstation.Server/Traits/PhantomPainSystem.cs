using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Traits.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Traits;

/// <summary>
/// Makes whoever has this whine in looc that they have phantom pain.
/// </summary>
public sealed class PhantomPainSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhantomPainComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<PhantomPainComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta == null
            || !args.DamageIncreased
            || ent.Comp.RequireOrigin && args.Origin == null)
            return;

        if (!PassesThreshold(ent.Comp, args.DamageDelta))
            return;

        TryComplain(ent);
    }

    private bool PassesThreshold(PhantomPainComponent component, DamageSpecifier delta)
    {
        var applicable = GetApplicableDamage(component, delta);
        if (applicable < component.DamageThreshold)
            return false;

        return true;
    }

    private FixedPoint2 GetApplicableDamage(PhantomPainComponent component, DamageSpecifier delta)
    {
        if (component.ValidDamageGroups == null)
            return delta.GetTotal();

        var total = FixedPoint2.Zero;
        foreach (var (group, value) in delta.GetDamagePerGroup(_prototype))
        {
            if (!component.ValidDamageGroups.Contains(group))
                continue;

            total += value;
        }

        return total;
    }

    private void TryComplain(Entity<PhantomPainComponent> ent)
    {
        // Yes they can still complain while crit
        if (_mobState.IsDead(ent)
            || !TryComp<ActorComponent>(ent, out var actor))
            return;

        if (ent.Comp.NextAllowedTime != null && _timing.CurTime < ent.Comp.NextAllowedTime
            || !_prototype.Resolve(ent.Comp.LineDataset, out var lines)
            || lines.Values.Count == 0)
            return;

        ent.Comp.NextAllowedTime = _timing.CurTime + ent.Comp.Cooldown;

        var message = Loc.GetString(_random.Pick(lines.Values));
        _chat.TrySendInGameOOCMessage(ent, message, InGameOOCChatType.Looc, false, player: actor.PlayerSession);
    }
}
