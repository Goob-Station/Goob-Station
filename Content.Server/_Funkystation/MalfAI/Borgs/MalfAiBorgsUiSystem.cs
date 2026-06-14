// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared._Funkystation.MalfAI;
using Content.Server._Funkystation.MalfAI.Laws;
using Content.Shared._Funkystation.MalfAI.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared._DV.Silicons.Laws;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI.Borgs;

/// <summary>
/// Server-side system that manages the Malf AI's borg control UI.
/// Allows the AI to view, sync, claim, and resync borgs.
/// </summary>
public sealed class MalfAiBorgsUiSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLaw = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly TimeSpan ClaimDelay = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan UiRefreshInterval = TimeSpan.FromSeconds(1);
    // I WILL REWORK THAT
    private readonly Dictionary<EntityUid, EntityUid> _activeClaims = [];

    private TimeSpan _nextUiRefresh;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        if (curTime < _nextUiRefresh)
            return;
        _nextUiRefresh = curTime + UiRefreshInterval;

        var query = EntityQueryEnumerator<MalfAiMarkerComponent>();
        while (query.MoveNext(out var ai, out _))
        {
            if (_ui.IsUiOpen(ai, MalfAiBorgsUiKey.Key))
                RefreshBorgsUi(ai);
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, OpenMalfAiBorgsUiActionEvent>(OnOpenBorgsUi);
        SubscribeLocalEvent<MalfAiMarkerComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiBorgsSetSyncMessage>(OnSetSync);
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiClaimBorgMessage>(OnClaimBorg);
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiClaimBorgDoAfterEvent>(OnClaimBorgDoAfter);
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiResyncAllBorgsMessage>(OnResyncAllBorgs);
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiOpenMasterLawsetMessage>(OnOpenMasterLawset);
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiBorgsJumpToBorgMessage>(OnJumpToBorg);
    }

    private void OnOpenBorgsUi(Entity<MalfAiMarkerComponent> ent, ref OpenMalfAiBorgsUiActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        _ui.TryToggleUi(ent.Owner, MalfAiBorgsUiKey.Key, actor.PlayerSession);
        args.Handled = true;
    }

    private void OnBoundUIOpened(Entity<MalfAiMarkerComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (args.UiKey is not MalfAiBorgsUiKey)
            return;

        RefreshBorgsUi(ent.Owner);
    }

    private void OnSetSync(Entity<MalfAiMarkerComponent> ent, ref MalfAiBorgsSetSyncMessage args)
    {
        var borg = GetEntity(args.Borg);
        if (!HasComp<BorgChassisComponent>(borg))
            return;

        // Only owned borgs can be synced.
        if (!TryComp<MalfAiControlledComponent>(borg, out var controlled) || controlled.Controller != ent.Owner)
            return;

        if (args.Synced)
        {
            var sync = EnsureComp<MalfBorgSyncToMasterComponent>(borg);
            sync.MalfAi = ent.Owner;
            ApplyMasterLawsToBorg(ent.Owner, borg);
        }
        else
        {
            RemComp<MalfBorgSyncToMasterComponent>(borg);
        }

        RefreshBorgsUi(ent.Owner);
    }

    private void OnClaimBorg(Entity<MalfAiMarkerComponent> ent, ref MalfAiClaimBorgMessage args)
    {
        // Only one claim at a time per AI.
        if (_activeClaims.ContainsKey(ent.Owner))
            return;

        var borg = GetEntity(args.Borg);
        if (!HasComp<BorgChassisComponent>(borg))
            return;

        if (!TryComp<SlavedBorgComponent>(borg, out var slaved) || slaved.Law != "ObeyAI")
            return;

        // Already owned — cannot be re-claimed.
        if (HasComp<MalfAiControlledComponent>(borg))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent.Owner, ClaimDelay,
            new MalfAiClaimBorgDoAfterEvent(args.Borg),
            eventTarget: ent.Owner)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
            _activeClaims[ent.Owner] = borg;
    }

    private void OnClaimBorgDoAfter(Entity<MalfAiMarkerComponent> ent, ref MalfAiClaimBorgDoAfterEvent args)
    {
        _activeClaims.Remove(ent.Owner);

        if (args.Cancelled || args.Handled)
            return;

        var borg = GetEntity(args.Borg);
        if (!HasComp<BorgChassisComponent>(borg))
            return;

        // Another AI may have claimed it during our DoAfter.
        if (HasComp<MalfAiControlledComponent>(borg))
            return;

        var controlled = EnsureComp<MalfAiControlledComponent>(borg);
        controlled.Controller = ent.Owner;
        Dirty(borg, controlled);

        args.Handled = true;
        RefreshBorgsUi(ent.Owner);
    }

    private void OnResyncAllBorgs(Entity<MalfAiMarkerComponent> ent, ref MalfAiResyncAllBorgsMessage args)
    {
        var query = EntityQueryEnumerator<MalfBorgSyncToMasterComponent>();
        while (query.MoveNext(out var borg, out var sync))
        {
            if (sync.MalfAi == ent.Owner)
                ApplyMasterLawsToBorg(ent.Owner, borg);
        }
    }

    private void OnOpenMasterLawset(Entity<MalfAiMarkerComponent> ent, ref MalfAiOpenMasterLawsetMessage args)
    {
        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        var eui = new MalfAiLawEui(_siliconLaw, EntityManager);
        _eui.OpenEui(eui, actor.PlayerSession);
        eui.OpenForMasterLawset(ent.Owner);
    }

    private void OnJumpToBorg(Entity<MalfAiMarkerComponent> ent, ref MalfAiBorgsJumpToBorgMessage args)
    {
        var borg = GetEntity(args.Borg);
        if (Deleted(borg))
            return;

        var core = Transform(ent.Owner).ParentUid;
        if (!TryComp<StationAiCoreComponent>(core, out var coreComp) || coreComp.RemoteEntity is not { } eye)
            return;

        _xforms.DropNextTo(eye, borg);
    }

    public void RefreshBorgsUi(EntityUid malfAi)
    {
        var borgs = GetBorgsForAi(malfAi);
        _ui.SetUiState(malfAi, MalfAiBorgsUiKey.Key, new MalfAiBorgsUiState(borgs));
    }

    private List<MalfAiBorgListEntry> GetBorgsForAi(EntityUid malfAi)
    {
        var result = new List<MalfAiBorgListEntry>();
        var xform = Transform(malfAi);

        var query = EntityQueryEnumerator<BorgChassisComponent, SiliconLawBoundComponent, TransformComponent>();
        while (query.MoveNext(out var borg, out _, out var lawBound, out var borgXform))
        {
            if (borgXform.MapID != xform.MapID)
                continue;

            if (!TryComp<SlavedBorgComponent>(borg, out var slaved) || slaved.Law != "ObeyAI")
                continue;

            var isSynced = HasComp<MalfBorgSyncToMasterComponent>(borg);
            var laws = _siliconLaw.GetLaws(borg, lawBound).Laws.Select(l => l.LawString).ToList();
            var health = 1f;

            if (TryComp<DamageableComponent>(borg, out var damageable))
            {
                var maxHealth = 100f;
                health = Math.Clamp(1f - (float)(damageable.TotalDamage / maxHealth), 0f, 1f);
            }

            var isOwned = TryComp<MalfAiControlledComponent>(borg, out var controlled);
            var ownedByMe = isOwned && controlled!.Controller == malfAi;
            var isBeingClaimed = _activeClaims.TryGetValue(malfAi, out var claimedBorg) && claimedBorg == borg;

            result.Add(new MalfAiBorgListEntry(
                GetNetEntity(borg),
                Name(borg),
                health,
                isSynced,
                laws,
                isOwned,
                ownedByMe,
                isBeingClaimed));
        }

        return result;
    }

    private void ApplyMasterLawsToBorg(EntityUid malfAi, EntityUid borg)
    {
        if (!TryComp<MalfMasterLawsetComponent>(malfAi, out var master))
            return;

        if (!TryComp<SiliconLawProviderComponent>(borg, out var provider))
            return;

        var laws = master.Laws.Select((law, idx) => new SiliconLaw
        {
            LawString = law,
            Order = idx + 1,
        }).ToList();

        _siliconLaw.SetLaws(laws, borg, provider.LawUploadSound);
    }
}
