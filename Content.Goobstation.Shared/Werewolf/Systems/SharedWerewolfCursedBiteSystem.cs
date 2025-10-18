using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This attack has a 50% chance to convert someone into a Werewolf.
/// This also does massive bleeding and damage.
/// </summary>
public abstract class SharedWerewolfCursedBiteSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfCursedBiteComponent, WerewolfCursedBiteEvent>(OnCursedBite);

        // Grabbing-related
        SubscribeLocalEvent<WerewolfCursedBiteComponent, PullStoppedMessage>(OnStopPulling);
        SubscribeLocalEvent<WerewolfCursedBiteComponent, ChokeholdEvent>(OnChokehold);

        SubscribeLocalEvent<HumanoidAppearanceComponent, CursedBiteUsedEvent>(OnHumanoidCursedBite);
        SubscribeLocalEvent<MindShieldComponent, CursedBiteUsedEvent>(OnMindShieldCursedBite);
        SubscribeLocalEvent<WerewolfComponent, CursedBiteUsedEvent>(OnWerewolfCursedBite);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var sentToBrazilQuery = EntityQueryEnumerator<WerewolfCursedBiteComponent>();
        while (sentToBrazilQuery.MoveNext(out _, out var comp))
        {
            if (!comp.Active)
                continue;

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.Active = false;
            comp.CanBeUsed = true;
        }
    }

    private void OnCursedBite(Entity<WerewolfCursedBiteComponent> ent, ref WerewolfCursedBiteEvent args)
    {
        if (!ent.Comp.CanBeUsed
            || args.Target != ent.Comp.Target)
            return;

        ent.Comp.Target = null;
        ent.Comp.CanBeUsed = false;

        if (_netManager.IsServer)
            TryModifyBleeding(args.Target, ent.Comp.BleedingAmount);

        if (ent.Comp.Damage != null)
            _damageable.TryChangeDamage(args.Target, ent.Comp.Damage);

        var ev = new CursedBiteUsedEvent(args.Target, ent.Owner);
        RaiseLocalEvent(args.Target, ref ev);

        if (!ev.Handled || ev.Cancelled)
            return;

        MakeWerewolf(args.Target, ent);
    }

    private void OnChokehold(Entity<WerewolfCursedBiteComponent> ent, ref ChokeholdEvent args)
    {
        if (!TryComp<PullerComponent>(ent.Owner, out var puller)
            || puller.Pulling == null)
            return;

        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.Duration;
        ent.Comp.Active = true;
        ent.Comp.Target = puller.Pulling;
    }

    private void OnStopPulling(Entity<WerewolfCursedBiteComponent> ent, ref PullStoppedMessage args)
    {
        if (!ent.Comp.Active)
            return;

        ent.Comp.Active = false;
        ent.Comp.Target = null;
        ent.Comp.CanBeUsed = false;
    }

    #region Cursed-Bite Handling

    private void OnHumanoidCursedBite(Entity<HumanoidAppearanceComponent> ent, ref CursedBiteUsedEvent args) =>
        args.Handled = true;
    private void OnMindShieldCursedBite(Entity<MindShieldComponent> ent, ref CursedBiteUsedEvent args) =>
        args.Cancelled = true;
    private void OnWerewolfCursedBite(Entity<WerewolfComponent> ent, ref CursedBiteUsedEvent args) =>
        args.Cancelled = true;

    #endregion

    /// <summary>
    ///  Makes the user into a werewolf
    /// </summary>
    /// <param name="target"></param> The target of the ability
    /// <param name="converter"></param> The user that converted the target into a werewolf
    /// <param name="pathLock"></param> If the target should be pathlocked to one form only.
    private void MakeWerewolf(EntityUid? target, Entity<WerewolfCursedBiteComponent> converter, bool pathLock = false)
    {
        if (target == null
            || !HasComp<HumanoidAppearanceComponent>(target)
            || converter.Comp.FormsToTransfer.Count <= 0
            || !_netManager.IsServer
            || !_random.Prob(0.5f)
            || !TryComp<MindComponent>(target, out var mind))
            return;

        MakeAntagWerewolf(target.Value);

        // Attach related forms
        if (!TryComp<WerewolfTransformComponent>(target.Value, out var werewolf))
            return;

        werewolf.UnlockedWerewolfForms.Clear();

        if (pathLock && TryComp<WerewolfMutationShopComponent>(target.Value, out var shop))
        {
            // just remove the shop
            RemCompDeferred<WerewolfMutationShopComponent>(target.Value);
            _actions.RemoveAction(shop.ActionEntity);
            werewolf.CurrentWerewolfForm = converter.Comp.PathLockedForm;
            return;
        }

        foreach (var form in converter.Comp.FormsToTransfer)
            werewolf.UnlockedWerewolfForms.Add(form);

        werewolf.CurrentWerewolfForm = converter.Comp.StartingForm;
    }

    protected virtual void TryModifyBleeding(EntityUid target, float amount) {}
    protected virtual void MakeAntagWerewolf(EntityUid target) {}
}
