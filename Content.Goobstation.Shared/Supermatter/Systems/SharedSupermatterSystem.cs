// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Content.Goobstation.Shared.Supermatter.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Supermatter.Systems;

public abstract class SharedSupermatterSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SupermatterComponent, ComponentStartup>(OnSupermatterStartup);
    }

    public enum SuperMatterSound : sbyte
    {
        Aggressive = 0,
        Delam = 1
    }

    public enum DelamType : sbyte
    {
        Explosion = 0,
        Singulo = 1,
        Tesla = 2,
        Cascade = 3 // save for later
    }
    #region Getters/Setters

    public void OnSupermatterStartup(EntityUid uid, SupermatterComponent comp, ComponentStartup args)
    {
    }

    #endregion Getters/Setters

    #region Serialization
    /// <summary>
    /// A state wrapper used to sync the supermatter between the server and client.
    /// </summary>
    [Serializable, NetSerializable]
    protected sealed class SupermatterComponentState : ComponentState
    {
        public SupermatterComponentState(SupermatterComponent supermatter)
        {
        }
    }

    #endregion Serialization

}