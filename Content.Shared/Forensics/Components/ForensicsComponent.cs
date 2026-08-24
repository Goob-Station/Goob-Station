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
