using Content.Goobstation.Common.Style;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Style
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class StyleCounterComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public StyleRank Rank = StyleRank.F;

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float CurrentPoints;

        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float CurrentMultiplier = 1.0f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float BaseDecayPerSecond = 10.0f;

        [ViewVariables]
        public TimeSpan LastEventTime;

        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan TimeToClear = TimeSpan.FromSeconds(5);

        [ViewVariables]
        public List<string> RecentEvents = new();

        [DataField("startingPoints")]
        public float StartingPoints = 100f;
    }
}
