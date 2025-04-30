// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Server.Xenoarchaeology.Equipment.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts;
using Content.Shared.Interaction;
using Content.Shared.Timing;
using Content.Shared.Verbs;

namespace Content.Server.Xenoarchaeology.Equipment.Systems;

public sealed class NodeScannerSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<NodeScannerComponent, BeforeRangedInteractEvent>(OnBeforeRangedInteract);
        SubscribeLocalEvent<NodeScannerComponent, GetVerbsEvent<UtilityVerb>>(AddScanVerb);
    }

    private void OnBeforeRangedInteract(EntityUid uid, NodeScannerComponent component, BeforeRangedInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not {} target)
            return;

        if (!TryComp<ArtifactComponent>(target, out var artifact) || artifact.CurrentNodeId == null)
            return;

        CreatePopup(uid, target, artifact);
        args.Handled = true;
    }

    private void AddScanVerb(EntityUid uid, NodeScannerComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess)
            return;

        if (!TryComp<ArtifactComponent>(args.Target, out var artifact) || artifact.CurrentNodeId == null)
            return;

        var verb = new UtilityVerb()
        {
            Act = () =>
            {
                CreatePopup(uid, args.Target, artifact);
            },
            Text = Loc.GetString("node-scan-tooltip")
        };

        args.Verbs.Add(verb);
    }

    private void CreatePopup(EntityUid uid, EntityUid target, ArtifactComponent artifact)
    {
        if (TryComp(uid, out UseDelayComponent? useDelay)
            && !_useDelay.TryResetDelay((uid, useDelay), true))
            return;

        _popupSystem.PopupEntity(Loc.GetString("node-scan-popup",
            ("id", $"{artifact.CurrentNodeId}")), target);
    }
}