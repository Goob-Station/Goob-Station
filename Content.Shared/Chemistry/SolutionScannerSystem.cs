// SPDX-FileCopyrightText: 2023 MisterMecky <mrmecky@hotmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Chemistry;

public sealed class SolutionScannerSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SolutionScannerComponent, SolutionScanEvent>(OnSolutionScanAttempt);
        SubscribeLocalEvent<SolutionScannerComponent, InventoryRelayedEvent<SolutionScanEvent>>((e, c, ev) => OnSolutionScanAttempt(e, c, ev.Args));
    }

    private void OnSolutionScanAttempt(EntityUid eid, SolutionScannerComponent component, SolutionScanEvent args)
    {
        args.CanScan = true;
    }
}

public sealed class SolutionScanEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool CanScan;
    public SlotFlags TargetSlots { get; } = SlotFlags.EYES;
}