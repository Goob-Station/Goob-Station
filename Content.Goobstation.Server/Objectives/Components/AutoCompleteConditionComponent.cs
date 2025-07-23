// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

// Automatically an assign objective as complete.

[RegisterComponent, Access(typeof(AutoCompleteConditionSystem))]
public sealed partial class AutoCompleteConditionComponent : Component
{
}
