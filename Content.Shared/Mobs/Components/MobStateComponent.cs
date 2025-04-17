// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Alfred Baumann <93665570+CheesePlated@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BuildTools <unconfigured@null.spigotmc.org>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Mobs.Components
{
    /// <summary>
    ///     When attached to an <see cref="DamageableComponent"/>,
    ///     this component will handle critical and death behaviors for mobs.
    ///     Additionally, it handles sending effects to clients
    ///     (such as blur effect for unconsciousness) and managing the health HUD.
    /// </summary>
    [RegisterComponent]
    [NetworkedComponent]
    [AutoGenerateComponentState]
    [Access(typeof(MobStateSystem), typeof(MobThresholdSystem))]
    public sealed partial class MobStateComponent : Component
    {
        //default mobstate is always the lowest state level
        [AutoNetworkedField, ViewVariables]
        public MobState CurrentState { get; set; } = MobState.Alive;

        [DataField]
        [AutoNetworkedField]
        public HashSet<MobState> AllowedStates = new()
            {
                MobState.Alive,
                MobState.Critical,
                MobState.Dead
            };
    }
}