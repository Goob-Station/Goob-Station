using System.Numerics;
using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Server.Stunnable;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles the Ambush ability of werewolf.
/// Leap at your victim, knocking them to the floor for some seconds.
/// </summary>
public sealed class WerewolfAmbushSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private  readonly StunSystem _stunSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfAmbushComponent, WerewolfAmbushEvent>(OnAmbush);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WerewolfAmbushComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active || comp.Target == null)
                continue;

            UpdateAmbush(comp.Target.Value, uid, comp);
        }
    }
    private void OnAmbush(Entity<WerewolfAmbushComponent> ent, ref WerewolfAmbushEvent args)
    {
        var target = args.Target;

        ent.Comp.Active = true;
        ent.Comp.Target = target;
        ent.Comp.StartTime = _timing.CurTime;
    }

    private void UpdateAmbush(EntityUid target, EntityUid user, WerewolfAmbushComponent component)
    {
        var userCoords = _transformSystem.GetWorldPosition(user);
        var targetCoords = _transformSystem.GetWorldPosition(target);

        var elapsed = (float) (_timing.CurTime - component.StartTime).TotalSeconds;
        var progress = elapsed / (float)component.LeapDuration.TotalSeconds;

        var newPos = Vector2.Lerp(userCoords, targetCoords, progress);

        if (progress >= 1)
        {
            component.Active = false;
            component.Target = null;
            StunNearby(user, component);
            return;
        }

        _transformSystem.SetWorldPosition(user, newPos);
    }

    #region Helper
    private void StunNearby(EntityUid user, WerewolfAmbushComponent component)
    {
        foreach (var ent in _lookup.GetEntitiesInRange(user, component.Range))
            _stunSystem.KnockdownOrStun(ent, component.StunDuration, false);
    }
    #endregion
}
