using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared._Goobstation.Bingle;
using Robust.Shared.Map;
using System.Numerics;
using Content.Shared._White.Overlays;
using Content.Server.Flash.Components;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Gravity;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Bingle;

public sealed class BingleSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<BingleComponent, ToggleNightVisionEvent>(OnNightvision);
        SubscribeLocalEvent<BingleComponent, ToggleCombatActionEvent>(OnCombatToggle);
        SubscribeLocalEvent<BingleComponent, SpawnBinglePitActionEvent>(OnSpawnBinglePitAction);
    }

    //ran by the pit to upgrade bingle damage
    public void UpgradeBingle(EntityUid uid, BingleComponent component)
    {
        if (component.Upgraded)
            return;

        var polyComp = EnsureComp<PolymorphableComponent>(uid);
        _polymorph.CreatePolymorphAction("BinglePolymorph",(uid, polyComp ));

        _popup.PopupEntity(Loc.GetString("bingle-upgrade-success"), uid, uid);
        component.Upgraded = true;
    }

    private void OnAttackAttempt(EntityUid uid, BingleComponent component, AttackAttemptEvent args)
    {
        //Prevent Friendly Bingle fire
        if (HasComp<BinglePitComponent>(args.Target) || HasComp<BingleComponent>(args.Target))
            args.Cancel();
    }

    private void OnNightvision(EntityUid uid, BingleComponent component, ToggleNightVisionEvent args)
    {
        if (!TryComp<FlashImmunityComponent>(uid, out var flashComp))
            return;

        flashComp.Enabled = !flashComp.Enabled;
    }

    private void OnCombatToggle(EntityUid uid, BingleComponent component, ToggleCombatActionEvent args)
    {
        if (!TryComp<CombatModeComponent>(uid, out var combat))
            return;
        _appearance.SetData(uid, BingleVisual.Combat, combat.IsInCombatMode);
    }

    public void OnSpawnBinglePitAction(EntityUid uid, BingleComponent component, SpawnBinglePitActionEvent args)
    {
        var cords = Transform(uid).Coordinates;
        var grid = Transform(uid).GridUid;

        //check if tile is valid
        if (!cords.IsValid(EntityManager) || grid == null)
        {       _popup.PopupEntity(Loc.GetString("bingle-cant-spawn-pit-here"), uid, uid);
                return; //you are in space and not on a grid.
        }

        if (TryComp<GravityComponent>(grid, out var gravityComp) && !gravityComp.Enabled)
        {
            _popup.PopupEntity(Loc.GetString("bingle-cant-spawn-no-gravity"), uid, uid);
            return; //Gravity is off
        }


        // check there is no bingle pit on tile
        var query = EntityQueryEnumerator<BinglePitComponent>();
        while (query.MoveNext(out var queryUid, out var pitcomp))
        {
            if (Transform(queryUid).Coordinates.Position == cords.Position)
            {
                _popup.PopupEntity(Loc.GetString("bingle-cant-spawn-to-close"), uid, uid);
                return; // some pit has the same cord as this so cancel the action
            }
        }

        // Spawn pit
        // add pit as this bingles pit
        var pit = Spawn("BinglePit", cords);
        component.MyPit = pit;
        _popup.PopupEntity(Loc.GetString("bingle-spawn-pit"), uid, uid);

        // remove action
        foreach (var ( actionId, comp ) in _actionsSystem.GetActions(uid))
        {
            if (!TryComp(actionId, out MetaDataComponent? metaData))
                continue;
            if (metaData.EntityPrototype != null && metaData.EntityPrototype == (EntProtoId)"ActionSpawnBinglePit")
                _actionsSystem.RemoveAction(actionId);
        }
    }
}

