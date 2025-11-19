// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Shared.Actions.Events;

public sealed class ActionUpgradeEvent : EntityEventArgs
{
    public int NewLevel;
    public EntityUid? ActionId;

    public ActionUpgradeEvent(int newLevel, EntityUid? actionId)
    {
        NewLevel = newLevel;
        ActionId = actionId;
    }
}
