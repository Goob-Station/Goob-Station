// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.NTR;

/// <summary>
/// A data structure for storing currently available bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public partial record struct NtrTaskData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The prototype containing information about the bounty.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<NtrTaskPrototype> Task { get; init; } = string.Empty;

    [DataField]
    public bool IsActive = false;

    [DataField]
    public bool IsAccepted = false;

    [DataField]
    public TimeSpan ActiveTime;

    [DataField]
    public bool IsCompleted = false;

    public NtrTaskData(NtrTaskPrototype task, string uniqueIdentifier)
    {
        Task = task.ID;
        Id = $"{task.IdPrefix}{uniqueIdentifier:D3}";
        IsActive = false;
        IsAccepted = false;
        ActiveTime = TimeSpan.Zero;
    }
    public NtrTaskData AsActive(TimeSpan time)
    {
        return this with { IsActive = true, ActiveTime = time };
    }
}
