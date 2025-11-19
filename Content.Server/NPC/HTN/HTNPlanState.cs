// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.NPC.HTN;

[Flags]
public enum HTNPlanState : byte
{
    TaskFinished = 1 << 0,

    PlanFinished = 1 << 1,
}
