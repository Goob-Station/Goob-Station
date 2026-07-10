// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Standing
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    [Access(typeof(StandingStateSystem), typeof(SharedCrawlUnderSystem))] // Pirate add: SharedCrawlUnderSystem
    public sealed partial class StandingStateComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public SoundSpecifier? DownSound { get; private set; } = new SoundCollectionSpecifier("BodyFall");

        [DataField, AutoNetworkedField]
        public bool Standing { get; set; } = true;

        /// <summary>
        /// Friction modifier applied to an entity in the downed state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DownFrictionMod = 0.4f;

        /// <summary>
        ///     List of fixtures that had their collision mask changed when the entity was downed.
        ///     Required for re-adding the collision mask.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> ChangedFixtures = new();

        // Pirate start - togglable under-table crawling
        [DataField, AutoNetworkedField]
        public bool IsCrawlingUnder = false;

        [DataField, AutoNetworkedField]
        public float CrawlingUnderSpeedModifier = 0.5f;

        [DataField, AutoNetworkedField]
        public int NormalDrawDepth = + 6; 

        [DataField, AutoNetworkedField]
        public int CrawlingUnderDrawDepth = - 3;

        [DataField, AutoNetworkedField]
        public TimeSpan LastCrawlToggleTime = TimeSpan.Zero;

        [DataField]
        public TimeSpan CrawlToggleCooldown = TimeSpan.FromSeconds(2);
        // Pirate end - togglable under-table crawling
    }
}
