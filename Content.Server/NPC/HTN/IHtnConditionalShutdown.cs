// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.NPC.HTN;

/// <summary>
/// Helper interface to run the appropriate shutdown for a particular task.
/// </summary>
public interface IHtnConditionalShutdown
{
    /// <summary>
    /// When to shut the task down.
    /// </summary>
    HTNPlanState ShutdownState { get; }

    /// <summary>
    /// Run whenever the <see cref="ShutdownState"/> specifies.
    /// </summary>
    void ConditionalShutdown(NPCBlackboard blackboard);
}
