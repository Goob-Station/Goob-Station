// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Content.Shared.Damage;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared._DV.Silicons.Laws;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Server-side system that manages the Malf AI's borg control UI.
/// Allows the AI to view, edit laws of, and sync borgs to its master lawset.
/// </summary>
public sealed class MalfAiBorgsUiSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLaw = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, OpenMalfAiBorgsUiActionEvent>(OnOpenBorgsUi);
        SubscribeLocalEvent<MalfAiMarkerComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiBorgsSetSyncMessage>(OnSetSync);
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

        // Only borgs slaved to the AI (SlavedBorg with the ObeyAI law, set in YAML on NT chassis)
        // can be synced; syndicate borgs, xenoborgs etc. lack it or obey something else.
        if (!TryComp<SlavedBorgComponent>(borg, out var slaved) || slaved.Law != "ObeyAI")
            return;

        if (args.Synced)
        {
            var sync = EnsureComp<MalfBorgSyncToMasterComponent>(borg);
            sync.MalfAi = ent.Owner;

            // Mark the borg as controlled by this AI (counts towards the control objective).
            var controlled = EnsureComp<MalfAiControlledComponent>(borg);
            controlled.Controller = ent.Owner;
            controlled.UniqueId ??= $"borg-{Guid.NewGuid():N}";
            Dirty(borg, controlled);

            // Immediately apply master laws
            ApplyMasterLawsToBorg(ent.Owner, borg);
        }
        else
        {
            RemComp<MalfBorgSyncToMasterComponent>(borg);
            RemComp<MalfAiControlledComponent>(borg);
        }

        RefreshBorgsUi(ent.Owner);
    }

    private void OnOpenMasterLawset(Entity<MalfAiMarkerComponent> ent, ref MalfAiOpenMasterLawsetMessage args)
    {
        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        var eui = new MalfAiLawEui(_siliconLaw, EntityManager);
        // The EUI must be opened (Player assigned) before StateDirty is called in OpenForMasterLawset.
        _eui.OpenEui(eui, actor.PlayerSession);
        eui.OpenForMasterLawset(ent.Owner);
    }

    private void OnJumpToBorg(Entity<MalfAiMarkerComponent> ent, ref MalfAiBorgsJumpToBorgMessage args)
    {
        var borg = GetEntity(args.Borg);
        if (Deleted(borg))
            return;

        // Move the AI's remote eye next to the selected borg, like funky-station does.
        // The AI is held inside the core; the core tracks the remote eye entity.
        var core = Transform(ent.Owner).ParentUid;
        if (!TryComp<StationAiCoreComponent>(core, out var coreComp) || coreComp.RemoteEntity is not { } eye)
            return;

        _xforms.DropNextTo(eye, borg);
    }

    private void RefreshBorgsUi(EntityUid malfAi)
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

            // Hide borgs not slaved to the AI (syndicate, xenoborg...) from the management UI entirely.
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

            result.Add(new MalfAiBorgListEntry(
                GetNetEntity(borg),
                Name(borg),
                health,
                isSynced,
                laws));
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
