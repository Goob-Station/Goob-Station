using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Alert;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Check if the Slasher is being observed by players.
/// </summary>
public sealed class SlasherObserverCheckSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherObserverCheckComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SlasherObserverCheckComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<SlasherObserverCheckComponent> ent, ref ComponentStartup args)
    {
        UpdateAlert(ent, false);
    }

    private void OnShutdown(Entity<SlasherObserverCheckComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent, ent.Comp.Alert);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsServer)
            return;

        var query = EntityQueryEnumerator<SlasherObserverCheckComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var isObserved = IsObservedByPlayers(uid, comp.Range);
            UpdateAlert((uid, comp), isObserved);
        }
    }

    private void UpdateAlert(Entity<SlasherObserverCheckComponent> ent, bool isObserved)
    {
        _alerts.ShowAlert(ent, ent.Comp.Alert, (short) (isObserved ? 1 : 0));
    }

    /// <summary>
    /// Checks if the slasher is being observed by any valid players.
    /// </summary>
    /// <param name="uid">The slasher entity.</param>
    /// <param name="range">The range to check for observers.</param>
    /// <returns>True if the slasher is being observed, false otherwise.</returns>
    public bool IsObservedByPlayers(EntityUid uid, float range)
    {
        foreach (var other in _lookup.GetEntitiesInRange(uid, range))
        {
            if (other == uid
                || !HasComp<EyeComponent>(other)
                || HasComp<GhostComponent>(other)
                || !HasComp<HumanoidAppearanceComponent>(other)
                || _mobState.IsDead(other)
                || _mobState.IsCritical(other)
                || TryComp<BlindableComponent>(other, out var blind) && blind.IsBlind)
                continue;

            if (_interaction.InRangeUnobstructed(other, uid, range, CollisionGroup.Opaque))
                return true;
        }

        return false;
    }
}
