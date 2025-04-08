// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.CollectiveMind;

[Prototype("collectiveMind")]
public sealed partial class CollectiveMindPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; } = default!;

    [DataField("name")]
    public string Name { get; private set; } = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    [DataField("keycode")]
    public char KeyCode { get; private set; } = '\0';

    [DataField("color")]
    public Color Color { get; private set; } = Color.Lime;

    [DataField("requiredComponents")]
    public List<string> RequiredComponents { get; set; } = new();

    [DataField("requiredTags")]
    public List<string> RequiredTags { get; set; } = new();

    [DataField]
    public bool ShowNames = true;
}