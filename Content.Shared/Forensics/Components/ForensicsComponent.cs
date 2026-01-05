// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Hagvan <22118902+Hagvan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Forensics.Components // Goob/Einstein Engins - Shared Forensics Component
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // Einstein Engines - Network that shit
    public sealed partial class ForensicsComponent : Component
    {
        [DataField, AutoNetworkedField] // Einstein Engines - Network that shit
        public HashSet<string> Fingerprints = new();

        [DataField, AutoNetworkedField] // Einstein Engines - Network that shit
        public HashSet<string> Fibers = new();

        [DataField, AutoNetworkedField] // Einstein Engines - Network that shit
        public HashSet<(string, TimeSpan)> DNAs = new(); // Goobstation

        [DataField, AutoNetworkedField] // Einstein Engines - Scent Tracking
        public string Scent = String.Empty;

        [DataField, AutoNetworkedField] // Einstein Engines - Network that shit
        public HashSet<string> Residues = new();

        /// <summary>
        /// How close you must be to wipe the prints/blood/etc. off of this entity
        /// </summary>
        [DataField("cleanDistance")]
        public float CleanDistance = 1.5f;

        /// <summary>
        /// Can the DNA be cleaned off of this entity?
        /// e.g. you can wipe the DNA off of a knife, but not a cigarette
        /// </summary>
        [DataField("canDnaBeCleaned")]
        public bool CanDnaBeCleaned = true;

        /// <summary>
        /// Moment in time next effect will be spawned - Einstein Engines
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan TargetTime = TimeSpan.Zero;
    }
}
