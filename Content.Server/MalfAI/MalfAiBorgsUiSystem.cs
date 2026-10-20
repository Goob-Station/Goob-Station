// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Robust.Server.GameObjects;
using Content.Shared.Silicons.Laws.Components; // SiliconLawsUiKey
using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Robust.Server.Player;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Utility;
using Content.Server.Silicons.StationAi;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;

namespace Content.Server.MalfAI;

public sealed class MalfAiBorgsUiSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLawSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly Content.Shared.Mobs.Systems.MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe on the AI-held entity for the action event to ensure delivery.
        SubscribeLocalEvent<Content.Shared.MalfAI.MalfAiMarkerComponent, OpenMalfAiBorgsUiActionEvent>(OnOpenUi);
        // Refresh UI when a borg becomes (un)linked, takes damage, or transponder status changes.
        SubscribeLocalEvent<MalfAiControlledComponent, ComponentStartup>(OnMalfBorgStart);
        SubscribeLocalEvent<MalfAiControlledComponent, ComponentShutdown>(OnMalfBorgShutdown);
        SubscribeLocalEvent<MalfAiControlledComponent, DamageChangedEvent>(OnMalfBorgDamaged);
        SubscribeLocalEvent<BorgTransponderComponent, ComponentShutdown>(OnTransponderShutdown);

        // Handle BUI messages from the AI-held owner; avoid MetaDataComponent in generics to prevent registration at startup.
        Subs.BuiEvents<Content.Shared.MalfAI.MalfAiMarkerComponent>(MalfAiBorgsUiKey.Key,
            subs =>
            {
                subs.Event<BoundUIOpenedEvent>(OnOpened);
                subs.Event<MalfAiBorgsUpdateLawsMessage>(OnUpdateLaws);
                subs.Event<MalfAiBorgsJumpToBorgMessage>(OnJumpToBorg);
                subs.Event<MalfAiOpenMasterLawsetMessage>(OnOpenMasterLawset);
                subs.Event<MalfAiBorgsSetSyncMessage>(OnSetSync);
            });
    }

    private void OnOpened(Entity<Content.Shared.MalfAI.MalfAiMarkerComponent> ai, ref BoundUIOpenedEvent args)
    {
        var state = BuildState(ai.Owner);
        _ui.SetUiState((ai.Owner, null), MalfAiBorgsUiKey.Key, state);
    }

    private void OnOpenUi(Entity<Content.Shared.MalfAI.MalfAiMarkerComponent> ai, ref OpenMalfAiBorgsUiActionEvent args)
    {
        if (args.Handled)
            return;

        if (!args.Performer.IsValid())
            return;

        // Open/Toggle the Borgs UI for the performer (AI entity)
        _ui.TryToggleUi((ai.Owner, null), MalfAiBorgsUiKey.Key, args.Performer);

        // Populate with current borgs controlled by this AI.
        var state = BuildState(ai.Owner);
        _ui.SetUiState((ai.Owner, null), MalfAiBorgsUiKey.Key, state);

        args.Handled = true;
    }

    private void OnSetSync(Entity<Content.Shared.MalfAI.MalfAiMarkerComponent> ai, ref MalfAiBorgsSetSyncMessage msg)
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

    private void OnUpdateLaws(Entity<Content.Shared.MalfAI.MalfAiMarkerComponent> ai, ref MalfAiBorgsUpdateLawsMessage msg)
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
        {
            if (_stationAi.TryGetCore(aiEntity, out var core) && core.Comp?.RemoteEntity != null)
                _playerManager.TryGetSessionByEntity(core.Comp.RemoteEntity.Value, out session);
        }

        if (session == null)
            return;

        var ui = new MalfAiLawEui(_siliconLawSystem, EntityManager);
        _eui.OpenEui(ui, session);
        TryComp<SiliconLawBoundComponent>(target.Value, out var lawBound);
        ui.UpdateLaws(lawBound, target.Value);
    }

    private void OnOpenMasterLawset(Entity<Content.Shared.MalfAI.MalfAiMarkerComponent> ai, ref MalfAiOpenMasterLawsetMessage msg)
    {
        // Open the editable Malf AI Laws EUI for the AI user, targeting the master lawset on the rule entity.
        if (!_playerManager.TryGetSessionByEntity(ai.Owner, out var session))
        {
            if (_stationAi.TryGetCore(ai.Owner, out var core) && core.Comp?.RemoteEntity != null)
                _playerManager.TryGetSessionByEntity(core.Comp.RemoteEntity.Value, out session);
        }

        if (session == null)
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
    /// Resolves an actor or pilot entity (such as StationAiEye, StationAiCore, or held brain) to the Malf AI brain.
    /// </summary>
    public bool TryGetMalfAi(EntityUid actor, out EntityUid malfAi)
    {
        if (HasComp<MalfAiMarkerComponent>(actor))
        {
            malfAi = actor;
            return true;
        }

        // If actor is the roving AI eye, find the core whose RemoteEntity matches this eye
        if (TryComp<StationAiEyeComponent>(actor, out _))
        {
            var coreQuery = AllEntityQuery<StationAiCoreComponent>();
            while (coreQuery.MoveNext(out var coreUid, out var coreComp))
            {
                if (coreComp.RemoteEntity == actor)
                {
                    if (_stationAi.TryGetHeld((coreUid, coreComp), out var held) &&
                        HasComp<MalfAiMarkerComponent>(held))
                    {
                        malfAi = held.Value;
                        return true;
                    }
                }
            }
        }

        // If actor is the core itself
        if (TryComp<StationAiCoreComponent>(actor, out var coreComp2) &&
            _stationAi.TryGetHeld((actor, coreComp2), out var heldCore) &&
            HasComp<MalfAiMarkerComponent>(heldCore))
        {
            malfAi = heldCore.Value;
            return true;
        }

        // If actor is held inside a core
        if (_stationAi.TryGetCore(actor, out var coreFromHeld) &&
            _stationAi.TryGetHeld(coreFromHeld, out var heldBrain) &&
            HasComp<MalfAiMarkerComponent>(heldBrain))
        {
            malfAi = heldBrain.Value;
            return true;
        }

        if (TryComp<Content.Shared.Mind.Components.MindContainerComponent>(actor, out var mindContainer) &&
            mindContainer.Mind != null)
        {
            var mindQuery = AllEntityQuery<MalfAiMarkerComponent, Content.Shared.Mind.Components.MindContainerComponent>();
            while (mindQuery.MoveNext(out var mUid, out _, out var mMindContainer))
            {
                if (mMindContainer.Mind == mindContainer.Mind)
                {
                    malfAi = mUid;
                    return true;
                }
            }
        }

        malfAi = default;
        return false;
    }

    /// <summary>
    /// Checks if a borg is controlled by the specified AI entity, including eye, core, and mind-based matching.
    /// </summary>
    public bool IsBorgControlledByAi(EntityUid controller, MalfAiControlledComponent borgControl)
    {
        if (borgControl.Controller == null)
        {
            var malfAiQuery = AllEntityQuery<MalfAiMarkerComponent>();
            var count = 0;
            EntityUid singleMalf = default;
            while (malfAiQuery.MoveNext(out var mUid, out _))
            {
                singleMalf = mUid;
                count++;
            }
            if (count == 1 && TryGetMalfAi(controller, out var myMalf) && myMalf == singleMalf)
            {
                borgControl.Controller = singleMalf;
                return true;
            }
            return false;
        }

        if (borgControl.Controller == controller)
            return true;

        if (TryGetMalfAi(controller, out var malfA) &&
            TryGetMalfAi(borgControl.Controller.Value, out var malfB) &&
            malfA == malfB)
        {
            return true;
        }

        if (TryGetMalfAi(controller, out var resA) && resA == borgControl.Controller.Value)
            return true;

        if (TryGetMalfAi(borgControl.Controller.Value, out var resB) && resB == controller)
            return true;

        // Check for mind-based matching (AI held vs brain entities)
        if (TryComp<Content.Shared.Mind.Components.MindContainerComponent>(controller, out var controllerMind) &&
            controllerMind.Mind != null &&
            TryComp<Content.Shared.Mind.Components.MindContainerComponent>(borgControl.Controller.Value, out var borgMind) &&
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
            var id = string.IsNullOrWhiteSpace(ctrl.UniqueId) ? ToPrettyString(uid) : ctrl.UniqueId;
            if (!string.Equals(id, uniqueId, StringComparison.Ordinal))
                continue;

            if (IsBorgControlledByAi(controller, ctrl))
                return uid;
        }

        return null;
    }

    private void OnJumpToBorg(Entity<Content.Shared.MalfAI.MalfAiMarkerComponent> ai, ref MalfAiBorgsJumpToBorgMessage msg)
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
        {
            var malfAiQuery = AllEntityQuery<MalfAiMarkerComponent>();
            var count = 0;
            EntityUid singleMalf = default;
            while (malfAiQuery.MoveNext(out var mUid, out _))
            {
                singleMalf = mUid;
                count++;
            }
            if (count == 1)
            {
                ent.Comp.Controller = singleMalf;
                Dirty(ent.Owner, ent.Comp);
                PushRefreshForControllerAndMind(singleMalf);
            }
            return;
        }
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

    private void OnTransponderShutdown(Entity<BorgTransponderComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<MalfAiControlledComponent>(ent.Owner, out var controlled) && controlled.Controller != null)
            PushRefreshForControllerAndMind(controlled.Controller.Value);
    }

    private MalfAiBorgsUiState BuildState(EntityUid controller)
    {
        var entries = new List<MalfAiBorgListEntry>();

        var query = AllEntityQuery<MalfAiControlledComponent>();
        while (query.MoveNext(out var uid, out var ctrl))
        {
            if (!IsBorgControlledByAi(controller, ctrl))
                continue;

            if (string.IsNullOrWhiteSpace(ctrl.UniqueId))
            {
                ctrl.UniqueId = Guid.NewGuid().ToString();
                Dirty(uid, ctrl);
            }
            var id = ctrl.UniqueId;
            var name = MetaData(uid).EntityName;

            SpriteSpecifier? sprite = null;
            if (TryComp<BorgTransponderComponent>(uid, out var trans) && trans.Sprite != null)
                sprite = trans.Sprite;
            else if (MetaData(uid).EntityPrototype is { } proto)
                sprite = new SpriteSpecifier.EntityPrototype(proto.ID);

            // Compute health as percentage relative to critical threshold (100 damage = 0%, 0 damage = 100%).
            var health = 1.0f;
            var isCritical = false;
            if (TryComp<DamageableComponent>(uid, out var dmg))
            {
                // Check if the borg is in critical state by comparing current damage to critical threshold
                if (_mobThreshold.TryGetThresholdForState(uid, Content.Shared.Mobs.MobState.Critical, out var critThreshold))
                {
                    isCritical = dmg.TotalDamage >= critThreshold;

                    // Calculate health as percentage remaining before critical threshold
                    // 0 damage = 100% health, critical threshold damage = 0% health
                    var damageRatio = (float)dmg.TotalDamage / critThreshold.Value.Float();
                    health = Math.Clamp(1.0f - damageRatio, 0f, 1f);
                }
            }
            var synced = HasComp<MalfBorgSyncToMasterComponent>(uid);
            entries.Add(new MalfAiBorgListEntry(id, name, sprite, health, synced, isCritical));
        }
        var result = new MalfAiBorgsUiState(entries);
        return result;
    }

    public void PushRefreshForControllerAndMind(EntityUid controller)
    {
        var targetEntity = TryGetMalfAi(controller, out var malf) ? malf : controller;
        var state = BuildState(targetEntity);

        // Refresh on the targetEntity entity if open
        if (_ui.IsUiOpen((targetEntity, null), MalfAiBorgsUiKey.Key))
            _ui.SetUiState((targetEntity, null), MalfAiBorgsUiKey.Key, state);

        if (targetEntity != controller && _ui.IsUiOpen((controller, null), MalfAiBorgsUiKey.Key))
            _ui.SetUiState((controller, null), MalfAiBorgsUiKey.Key, state);

        // Also refresh on any other entity with the same mind (AiHeld/Brain counterpart)
        if (TryComp<Content.Shared.Mind.Components.MindContainerComponent>(targetEntity, out var mindCont) && mindCont.Mind != null)
        {
            var targetMind = mindCont.Mind.Value;
            var mindQuery = AllEntityQuery<Content.Shared.Mind.Components.MindContainerComponent>();
            while (mindQuery.MoveNext(out var otherEnt, out var otherMindCont))
            {
                if (otherEnt == targetEntity || otherEnt == controller)
                    continue;
                if (otherMindCont.Mind != targetMind)
                    continue;
                if (_ui.IsUiOpen((otherEnt, null), MalfAiBorgsUiKey.Key))
                    _ui.SetUiState((otherEnt, null), MalfAiBorgsUiKey.Key, state);
            }
        }
    }
}
