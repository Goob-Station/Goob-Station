// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Shitmed.Medical.Surgery;

public enum StepInvalidReason
{
    None,
    MissingSkills,
    NeedsOperatingTable,
    Armor,
    MissingTool,
    SurgeryInvalid,
    MissingPreviousSteps,
    StepCompleted,
    ToolInvalid,
    DoAfterFailed
}
