using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.Showers
{
    /// <summary>
    /// A shower that draws from an internal water tank. While running it sprays water onto its own
    /// tile every tick (wetting people/items and diluting stains through the water reaction), and it
    /// passively refills the tank when idle. Runs dry and shuts off if the tank empties.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class ShowerComponent : Component
    {
        /// <summary>
        /// Whether the user wants the shower running. It can still shut itself off if it runs dry.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool ToggleShower;

        [DataField("enableShowerSound")]
        public SoundSpecifier EnableShowerSound = new SoundPathSpecifier("/Audio/_Pirate/Ambience/Objects/shower_enable.ogg");

        public EntityUid? PlayingStream;

        [DataField("loopingSound")]
        public SoundSpecifier LoopingSound = new SoundPathSpecifier("/Audio/_Pirate/Ambience/Objects/shower_running.ogg");

        /// <summary>Name of the internal water tank solution.</summary>
        [DataField]
        public string SolutionName = "tank";

        /// <summary>Reagent the tank holds, sprays, and regenerates.</summary>
        [DataField]
        public string Reagent = "Water";

        /// <summary>
        /// Radius of the on-tile lookup. Results are still filtered to the shower's own tile, so this
        /// only needs to reach the tile's corners; a plain radius would bleed onto neighbours.
        /// </summary>
        [DataField]
        public float SprayRange = 0.8f;

        /// <summary>How often the shower sprays and regenerates, in seconds.</summary>
        [DataField]
        public float SprayInterval = 1f;

        [ViewVariables]
        public float SprayAccumulator;

        /// <summary>Water drawn from the tank and applied to the tile each spray tick.</summary>
        [DataField]
        public FixedPoint2 SprayAmount = FixedPoint2.New(5);

        /// <summary>Water regenerated into the tank each tick while running.</summary>
        [DataField]
        public FixedPoint2 RegenOn = FixedPoint2.New(1);

        /// <summary>Water regenerated into the tank each tick while off (refills faster).</summary>
        [DataField]
        public FixedPoint2 RegenOff = FixedPoint2.New(3);
    }


    [Serializable, NetSerializable]
    public enum ShowerVisuals : byte
    {
        ShowerVisualState,
    }

    [Serializable, NetSerializable]
    public enum ShowerVisualState : byte
    {
        Off,
        On
    }
}
