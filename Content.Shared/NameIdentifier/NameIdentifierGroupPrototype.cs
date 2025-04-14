// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.NameIdentifier;

[Prototype("nameIdentifierGroup")]
public sealed partial class NameIdentifierGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Should the identifier become the full name, or just append?
    /// </summary>
    [DataField("fullName")]
    public bool FullName = false;

    [DataField("prefix")]
    public string? Prefix;

    [DataField("maxValue")]
    public int MaxValue = 1000;

    [DataField("minValue")]
    public int MinValue = 0;
}