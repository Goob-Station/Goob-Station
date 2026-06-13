// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.CollectiveMind;

[Prototype]
public sealed partial class CollectiveMindPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = string.Empty;

    [ViewVariables]
    public string LocalizedName => Loc.GetString(Name);

    [DataField("keycode")]
    public char KeyCode = '\0';

    [DataField]
    public Color Color = Color.Lime;

    [DataField]
    public List<string> RequiredComponents = new();

    [DataField]
    public List<string> RequiredTags = new();

    [DataField]
    public bool ShowNames = true;
}
