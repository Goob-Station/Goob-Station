using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory.Events;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Gangwars.Systems;

/// <summary>
/// Periodically checks whether the gangmember is standing within range
/// of any gang territory and updates the gang territory alert accordingly.
/// </summary>
public sealed class GangMemberSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly GangwarRuleSystem _gangwarRule = default!;

    private const short GangBonusDark = 1; // not wearing gang clothes
    private const short GangBonusNormal = 2; // wearing gang clothes, normal bonus
    private const short GangBonusHealing = 3; // wearing gang clothes, standing in gang territory
    private const short GangBonusHealingFast = 4; // wearing gang clothes, near locker / buffed territory

    private readonly HashSet<Entity<GangTerritoryComponent>> _nearbyTerritories = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangMemberComponent, ComponentStartup>(OnMemberStartup);
        SubscribeLocalEvent<GangMemberComponent, ComponentShutdown>(OnMemberShutdown);
        SubscribeLocalEvent<GangMemberComponent, DidEquipEvent>(OnMemberEquip);
        SubscribeLocalEvent<GangMemberComponent, DidUnequipEvent>(OnMemberUnequip);
        SubscribeLocalEvent<GangMemberComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<GangMemberComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<GangMemberComponent, GangMemberToggleOverlayEvent>(OnToggleOverlay);
    }

    private void OnMemberStartup(Entity<GangMemberComponent> ent, ref ComponentStartup args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.TerritoryAlert, severity: GangBonusDark);
        _actions.AddAction(ent.Owner, ref ent.Comp.ToggleOverlayActionEnt, ent.Comp.ToggleOverlayAction);
        Dirty(ent);
    }

    private void OnMemberShutdown(Entity<GangMemberComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.TerritoryAlert);
        _actions.RemoveAction(ent.Owner, ent.Comp.ToggleOverlayActionEnt);
        Dirty(ent);
    }

    private void OnToggleOverlay(Entity<GangMemberComponent> ent, ref GangMemberToggleOverlayEvent args)
    {
        ent.Comp.OverlayVisible = !ent.Comp.OverlayVisible;
        args.Handled = true;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<GangMemberComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var memberComp, out var memberXform))
        {
            var curTime = _timing.CurTime;
            if (curTime < memberComp.NextCheck)
                continue;

            memberComp.NextCheck = curTime + TimeSpan.FromSeconds(memberComp.CheckInterval);

            if (!memberComp.WearingGangClothes)
            {
                _alerts.ShowAlert(uid, memberComp.TerritoryAlert, severity: GangBonusDark);

                if (memberComp.IsInTerritory)
                {
                    memberComp.IsInTerritory = false;
                    Dirty(uid, memberComp);
                }

                continue;
            }

            _nearbyTerritories.Clear();
            _lookup.GetEntitiesInRange(memberXform.Coordinates, memberComp.MaxTerritoryRangeCheck, _nearbyTerritories);

            var memberPos = _transform.GetWorldPosition(memberXform);
            var inTerritory = false;
            var inBuffedTerritory = false;
            foreach (var territory in _nearbyTerritories)
            {
                var dist = (memberPos - _transform.GetWorldPosition(territory.Owner)).Length();
                if (dist <= territory.Comp.TerritoryRadius)
                {
                    inTerritory = true;
                    if (territory.Comp.BuffedHealing)
                        inBuffedTerritory = true;
                }
            }

            var healSpec = inBuffedTerritory
                ? memberComp.BuffedHealAmount
                : memberComp.HealAmount;

            _damageable.TryChangeDamage(uid, healSpec, ignoreResistances: true);

            if (_net.IsServer && _random.Prob(memberComp.BleedReductionChance))
                _bloodstream.TryModifyBleedAmount(uid, memberComp.BleedReductionAmount);

            var severity = inBuffedTerritory
                ? GangBonusHealingFast
                : inTerritory
                    ? GangBonusHealing
                    : GangBonusNormal;
            _alerts.ShowAlert(uid, memberComp.TerritoryAlert, severity: severity);

            if (memberComp.IsInTerritory == inTerritory)
                continue;

            memberComp.IsInTerritory = inTerritory;
            Dirty(uid, memberComp);
        }
    }

    private void OnMemberEquip(Entity<GangMemberComponent> ent, ref DidEquipEvent args) =>
        UpdateGangClothingBuff(ent);

    private void OnMemberUnequip(Entity<GangMemberComponent> ent, ref DidUnequipEvent args) =>
        UpdateGangClothingBuff(ent);

    private void UpdateGangClothingBuff(Entity<GangMemberComponent> ent)
    {
        ent.Comp.WearingGangClothes = _gangwarRule.IsWearingGangOutfit(ent.Owner, ent.Comp.Gang);
        Dirty(ent);
    }

    private void OnBeforeStaminaDamage(Entity<GangMemberComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (!ent.Comp.IsInTerritory || !ent.Comp.WearingGangClothes)
            return;

        args.Value *= ent.Comp.StaminaBuff;
    }

    private void OnDamageModify(Entity<GangMemberComponent> ent, ref DamageModifyEvent args)
    {
        if (!ent.Comp.IsInTerritory || !ent.Comp.WearingGangClothes)
            return;

        args.Damage *= ent.Comp.DefenseBuff;
    }
}
