// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Components;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared.Database;

namespace Content.Goobstation.Server.Destructible.Thresholds.Behaviors
{
    [Serializable]
    [DataDefinition]
    public sealed partial class KillSignCauseBehavior : IThresholdBehavior
    {
        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            if (cause == null)
                return;

            var causeVal = cause.Value;

            if (!system.EntityManager.TryGetComponent<KillSignComponent>(causeVal, out var killsignComp))
            {
                system.EntityManager.AddComponent<KillSignComponent>(causeVal);
                system._adminLogger.Add(LogType.Trigger, LogImpact.High, $"{system.EntityManager.ToPrettyString(causeVal):entity} was Killsigned because they broke a Christmas tree: {system.EntityManager.ToPrettyString(owner):entity}.");
            }
        }
    }
}