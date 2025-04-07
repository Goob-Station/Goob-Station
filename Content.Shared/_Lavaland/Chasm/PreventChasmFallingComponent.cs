// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._Lavaland.Chasm;

[RegisterComponent]
public sealed partial class PreventChasmFallingComponent : Component
{
    [DataField]
    public bool DeleteOnUse = true;
}