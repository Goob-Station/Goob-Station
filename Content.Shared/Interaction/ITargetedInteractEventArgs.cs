// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Shared.Interaction
{
    public interface ITargetedInteractEventArgs
    {
        /// <summary>
        /// Performer of the attack
        /// </summary>
        EntityUid User { get; }
        /// <summary>
        /// Target of the attack
        /// </summary>
        EntityUid Target { get; }

    }
}
