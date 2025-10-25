// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Components;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.Fluids;

[Serializable, NetSerializable]
public sealed partial class AbsorbantDoAfterEvent : DoAfterEvent
{
    [DataField("solution", required: true)]
    public string TargetSolution = default!;

    [DataField("message", required: true)]
    public string Message = default!;

    [DataField("sound", required: true)]
    public SoundSpecifier Sound = default!;

    [DataField("transferAmount", required: true)]
    public FixedPoint2 TransferAmount;

    private AbsorbantDoAfterEvent()
    {
    }

    public AbsorbantDoAfterEvent(string targetSolution, string message, SoundSpecifier sound, FixedPoint2 transferAmount)
    {
        TargetSolution = targetSolution;
        Message = message;
        Sound = sound;
        TransferAmount = transferAmount;
    }

    public override DoAfterEvent Clone() => this;
}

/// <summary>
/// Raised when trying to spray something, for example a fire extinguisher.
/// </summary>
[ByRefEvent]
public record struct SprayAttemptEvent(EntityUid User, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}

public sealed partial class SpilledOnEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;

    public EntityUid Source;
    public Solution Solution;

    public SpilledOnEvent(EntityUid source, Solution solution)
    {
        Source = source;
        Solution = solution;
    }
}
