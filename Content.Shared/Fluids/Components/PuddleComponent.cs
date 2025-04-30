// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Fluids.Components
{
    /// <summary>
    /// Puddle on a floor
    /// </summary>
    [RegisterComponent, NetworkedComponent, Access(typeof(SharedPuddleSystem))]
    public sealed partial class PuddleComponent : Component
    {
        [DataField]
        public SoundSpecifier SpillSound = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");

        [DataField]
        public FixedPoint2 OverflowVolume = FixedPoint2.New(20);

        [DataField("solution")] public string SolutionName = "puddle";

        [ViewVariables]
        public Entity<SolutionComponent>? Solution;

        // Corvax-Next-Footprints-Start
        [DataField]
        public bool AffectsMovement = true;

        [DataField]
        public bool AffectsSound = true;
        // Corvax-Next-Footprints-End
    }
}