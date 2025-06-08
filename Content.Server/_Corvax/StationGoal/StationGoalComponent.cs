using Robust.Shared.Prototypes;

namespace Content.Server.Corvax.StationGoal
{
    /// <summary>
    ///     if attached to a station prototype, will send the station a random goal from the list
    /// </summary>
    [RegisterComponent]
    public sealed partial class StationGoalComponent : Component
    {
        [DataField]
        public List<ProtoId<StationGoalPrototype>> Goals = new();
    }
}
