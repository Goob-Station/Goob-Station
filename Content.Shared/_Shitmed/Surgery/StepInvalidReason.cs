// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Shitmed.Medical.Surgery;

public enum StepInvalidReason
{
    None,
    MissingSkills,
    NeedsOperatingTable,
    Armor,
    MissingTool,
}