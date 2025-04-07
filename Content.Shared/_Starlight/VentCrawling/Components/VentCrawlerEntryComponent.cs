// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._Starlight.VentCrawling.Components;

[RegisterComponent]
public sealed partial class VentCrawlerEntryComponent : Component
{
    [DataField]
    public string HolderPrototypeId = "VentCrawlerHolder";
}