// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Robust.Server.GameObjects;
using Content.Shared.Silicons.Laws.Components;
using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Robust.Server.Player;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Utility;
using Content.Server.Silicons.StationAi;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Mobs.Systems;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mind.Components;
using Content.Server._Funkystation.MalfAI.Components;
using Content.Shared._Funkystation.MalfAI.Components;

namespace Content.Server._Funkystation.MalfAI;

public sealed class MalfAiBorgsUiSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLawSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe on the AI-held entity for the action event to ensure delivery.
        SubscribeLocalEvent<StationAiHeldComponent, OpenMalfAiBorgsUiActionEvent>(OnOpenUi);

        // Refresh UI when a borg becomes (un)linked and when it takes damage.
        SubscribeLocalEvent<MalfAiControlledComponent, ComponentStartup>(OnMalfBorgStart);
        SubscribeLocalEvent<MalfAiControlledComponent, ComponentShutdown>(OnMalfBorgShutdown);
        SubscribeLocalEvent<MalfAiControlledComponent, Content.Shared.Damage.DamageChangedEvent>(OnMalfBorgDamaged);

        // Handle BUI messages from the AI-held owner; avoid MetaDataComponent in generics to prevent registration at startup.
        Subs.BuiEvents<StationAiHeldComponent>(MalfAiBorgsUiKey.Key, subs =>
        {
            subs.Event<MalfAiBorgsUpdateLawsMessage>(OnUpdateLaws);
            subs.Event<MalfAiBorgsJumpToBorgMessage>(OnJumpToBorg);
            subs.Event<MalfAiOpenMasterLawsetMessage>(OnOpenMasterLawset);
            subs.Event<MalfAiBorgsSetSyncMessage>(OnSetSync);
        });
    }

    private void OnOpenUi(Entity<StationAiHeldComponent> ai, ref OpenMalfAiBorgsUiActionEvent args)
    {
        if (args.Handled)
            return;

        if (!args.Performer.IsValid())
            return;

        // Open/Toggle the Borgs UI for the performer (AI entity)
        _ui.TryToggleUi((ai.Owner, null), MalfAiBorgsUiKey.Key, args.Performer);

        // Populate with current borgs controlled by this AI.
        var state = BuildState(args.Performer);
        _ui.SetUiState((ai.Owner, null), MalfAiBorgsUiKey.Key, state);

        args.Handled = true;
    }

    private void OnSetSync(Entity<StationAiHeldComponent> ai, ref MalfAiBorgsSetSyncMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg.UniqueId))
            return;

        var aiEntity = ai.Owner;

        // Resolve target borg by UniqueId under this AI (or same mind).
        var target = FindBorgByUniqueId(aiEntity, msg.UniqueId);
        if (target == null)
            return;

        if (msg.Enabled)
        {
            EnsureComp<MalfBorgSyncToMasterComponent>(target.Value);

            // Immediately apply the current master lawset to this borg by reading from the rule's bound container.
            var ruleQuery = AllEntityQuery<MalfMasterLawsetComponent, SiliconLawBoundComponent>();
            while (ruleQuery.MoveNext(out var ruleUid, out _, out var bound))
            {
                var masterSet = _siliconLawSystem.GetLaws(ruleUid, bound).Laws;

                if (TryComp<SiliconLawProviderComponent>(target.Value, out var borgProvider))
                    _siliconLawSystem.SetLaws(masterSet, target.Value, borgProvider.LawUploadSound);
                else
                    _siliconLawSystem.SetLaws(masterSet, target.Value);
                break;
            }
        }
        else
        {
            RemComp<MalfBorgSyncToMasterComponent>(target.Value);
        }

        // Refresh UI to update disabled state.
        PushRefreshForControllerAndMind(aiEntity);
    }

    private void OnUpdateLaws(Entity<StationAiHeldComponent> ai, ref MalfAiBorgsUpdateLawsMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg.UniqueId))
            return;

        var aiEntity = ai.Owner;

        // If the borg is synced to master, ignore manual edit requests.
        var borg = FindBorgByUniqueId(aiEntity, msg.UniqueId);
        if (borg != null && HasComp<MalfBorgSyncToMasterComponent>(borg.Value))
            return;

        // Find the borg with matching UniqueId controlled by this AI (or same mind).
        var target = FindBorgByUniqueId(aiEntity, msg.UniqueId);
        if (target == null)
            return;

        // Open the editable Malf AI Laws EUI for the AI user (separate from admin EUI).
        if (!_playerManager.TryGetSessionByEntity(aiEntity, out var session))
            return;

        var ui = new MalfAiLawEui(_siliconLawSystem, EntityManager);
        _eui.OpenEui(ui, session);
        TryComp<SiliconLawBoundComponent>(target.Value, out var lawBound);
        ui.UpdateLaws(lawBound, target.Value);
    }

    private void OnOpenMasterLawset(Entity<StationAiHeldComponent> ai, ref MalfAiOpenMasterLawsetMessage msg)
    {
        // Open the editable Malf AI Laws EUI for the AI user, targeting the master lawset on the rule entity.
        if (!_playerManager.TryGetSessionByEntity(ai.Owner, out var session))
            return;

        // Find the rule entity that holds the master lawset.
        EntityUid? ruleUid = null;
        MalfMasterLawsetComponent? masterLawset = null;
        var ruleQuery = AllEntityQuery<MalfMasterLawsetComponent>();
        if (ruleQuery.MoveNext(out var uid, out var comp))
        {
            ruleUid = uid;
            masterLawset = comp;
        }

        if (ruleUid == null || masterLawset == null)
        {
            return;
        }

        // Ensure a SiliconLawBoundComponent exists on the rule entity so the EUI has a valid target to read/write.
        var lawBound = EnsureComp<SiliconLawBoundComponent>(ruleUid.Value);

        var ui = new MalfAiLawEui(_siliconLawSystem, EntityManager);
        _eui.OpenEui(ui, session);

        // Pass the actual law container so Save writes correctly.
        ui.UpdateLaws(lawBound, ruleUid.Value);
    }

    /// <summary>
    /// Checks if a borg is controlled by the specified AI entity, including mind-based matching.
    /// </summary>
    private bool IsBorgControlledByAi(EntityUid controller, MalfAiControlledComponent borgControl)
    {
        if (borgControl.Controller == controller)
            return true;

        // Check for mind-based matching (AI held vs brain entities)
        if (borgControl.Controller != null &&
            TryComp<MindContainerComponent>(controller, out var controllerMind) &&
            controllerMind.Mind != null &&
            TryComp<MindContainerComponent>(borgControl.Controller.Value, out var borgMind) &&
            borgMind.Mind == controllerMind.Mind)
        {
            return true;
        }

        return false;
    }

    private EntityUid? FindBorgByUniqueId(EntityUid controller, string uniqueId)
    {
        var query = AllEntityQuery<MalfAiControlledComponent>();
        while (query.MoveNext(out var uid, out var ctrl))
        {
            if (!string.Equals(ctrl.UniqueId, uniqueId, StringComparison.Ordinal))
                continue;

            if (IsBorgControlledByAi(controller, ctrl))
                return uid;
        }

        return null;
    }

    private void OnJumpToBorg(Entity<StationAiHeldComponent> ai, ref MalfAiBorgsJumpToBorgMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg.UniqueId))
            return;

        var aiEntity = ai.Owner;

        // Resolve target borg controlled by this AI (or same mind)
        var target = FindBorgByUniqueId(aiEntity, msg.UniqueId);
        if (target == null)
            return;

        // Move the AI remote eye next to the selected borg.
        if (!_stationAi.TryGetCore(aiEntity, out var core) || core.Comp?.RemoteEntity == null)
            return;

        _xforms.DropNextTo(core.Comp.RemoteEntity.Value, target.Value);
    }

    private void OnMalfBorgStart(Entity<MalfAiControlledComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.Controller is not { } controller)
            return;
        // If UI is open for this AI or any entity with the same Mind, refresh.
        PushRefreshForControllerAndMind(controller);
    }

    private void OnMalfBorgShutdown(Entity<MalfAiControlledComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Controller is not { } controller)
            return;
        PushRefreshForControllerAndMind(controller);
    }

    private void OnMalfBorgDamaged(Entity<MalfAiControlledComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.Controller is not { } controller)
            return;
        PushRefreshForControllerAndMind(controller);
    }

    private MalfAiBorgsUiState BuildState(EntityUid controller)
    {
        var entries = new List<MalfAiBorgListEntry>();

        var query = AllEntityQuery<MalfAiControlledComponent>();
        while (query.MoveNext(out var uid, out var ctrl))
        {
            if (!IsBorgControlledByAi(controller, ctrl))
                continue;

            var id = string.IsNullOrWhiteSpace(ctrl.UniqueId) ? ToPrettyString(uid) : ctrl.UniqueId!;
            var name = MetaData(uid).EntityName;
            SpriteSpecifier? sprite = null;
            if (TryComp<BorgTransponderComponent>(uid, out var trans))
                sprite = trans.Sprite;
            // Compute health as percentage relative to critical threshold (100 damage = 0%, 0 damage = 100%).
            var health = 1.0f;
            var isCritical = false;
            if (TryComp<DamageableComponent>(uid, out var dmg))
            {
                // Check if the borg is in critical state by comparing current damage to critical threshold
                if (_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var critThreshold))
                {
                    isCritical = dmg.TotalDamage >= critThreshold;

                    // Calculate health as percentage remaining before critical threshold
                    // 0 damage = 100% health, critical threshold damage = 0% health
                    var damageRatio = (float) dmg.TotalDamage / critThreshold.Value.Float();
                    health = Math.Clamp(1.0f - damageRatio, 0f, 1f);
                }
            }
            var synced = HasComp<MalfBorgSyncToMasterComponent>(uid);
            entries.Add(new MalfAiBorgListEntry(id, name, sprite, health, synced, isCritical));
        }
        var result = new MalfAiBorgsUiState(entries);
        return result;
    }

    private void PushRefreshForControllerAndMind(EntityUid controller)
    {
        var state = BuildState(controller);
        // Refresh on the controller entity if open
        if (_ui.IsUiOpen((controller, null), MalfAiBorgsUiKey.Key))
            _ui.SetUiState((controller, null), MalfAiBorgsUiKey.Key, state);

        // Also refresh on any other entity with the same mind (AiHeld/Brain counterpart)
        if (TryComp<MindContainerComponent>(controller, out var mindCont) && mindCont.Mind != null)
        {
            var targetMind = mindCont.Mind.Value;
            var mindQuery = AllEntityQuery<MindContainerComponent>();
            while (mindQuery.MoveNext(out var otherEnt, out var otherMindCont))
            {
                if (otherEnt == controller)
                    continue;
                if (otherMindCont.Mind != targetMind)
                    continue;
                if (_ui.IsUiOpen((otherEnt, null), MalfAiBorgsUiKey.Key))
                    _ui.SetUiState((otherEnt, null), MalfAiBorgsUiKey.Key, state);
            }
        }
    }
}
