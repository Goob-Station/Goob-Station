// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 themias <89101928+themias@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Forensics;

[Serializable, NetSerializable]
public sealed partial class ForensicScannerDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ForensicPadDoAfterEvent : DoAfterEvent
{
    [DataField("sample", required: true)] public  string Sample = default!;

    private ForensicPadDoAfterEvent()
    {
    }

    public ForensicPadDoAfterEvent(string sample)
    {
        Sample = sample;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class CleanForensicsDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
/// An event to apply DNA evidence from a donor onto some recipient.
/// </summary>
[ByRefEvent]
public record struct TransferDnaEvent()
{
    /// <summary>
    /// The entity donating the DNA.
    /// </summary>
    public EntityUid Donor;

    /// <summary>
    /// The entity receiving the DNA.
    /// </summary>
    public EntityUid Recipient;

    /// <summary>
    /// Can the DNA be cleaned off?
    /// </summary>
    public bool CanDnaBeCleaned = true;
}

/// <summary>
/// An event to generate and act upon new DNA for an entity.
/// </summary>
[ByRefEvent]
public record struct GenerateDnaEvent()
{
    /// <summary>
    /// The entity getting new DNA.
    /// </summary>
    public EntityUid Owner;

    /// <summary>
    /// The generated DNA.
    /// </summary>
    public required string DNA;
}
