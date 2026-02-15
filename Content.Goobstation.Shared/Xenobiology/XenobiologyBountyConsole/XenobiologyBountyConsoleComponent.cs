using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenobiologyBountyConsoleComponent : Component
{
    /// <summary>
    /// The sound made when the bounty is fulfilled.
    /// </summary>
    [DataField]
    public SoundSpecifier FulfillSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// The sound made when bounty skipping is denied due to lacking access, or if
    /// the items needed are not present.
    /// </summary>
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_two.ogg");

    /// <summary>
    /// The time at which the console will be able to make the denial sound again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextDenySoundTime = TimeSpan.Zero;

    /// <summary>
    /// The time between playing a denial sound.
    /// </summary>
    [DataField]
    public TimeSpan DenySoundDelay = TimeSpan.FromSeconds(2);
}

[NetSerializable, Serializable]
public sealed class XenobiologyBountyConsoleState(
    List<XenobiologyBountyData> bounties,
    List<XenobiologyBountyHistoryData> history,
    TimeSpan untilNextSkip,
    TimeSpan untilNextGlobalRefresh)
    : BoundUserInterfaceState
{
    public List<XenobiologyBountyData> Bounties = bounties;
    public List<XenobiologyBountyHistoryData> History = history;
    public TimeSpan UntilNextSkip = untilNextSkip;
    public TimeSpan UntilNextGlobalRefresh = untilNextGlobalRefresh;
}

[Serializable, NetSerializable]
public sealed class BountyFulfillMessage(string bountyId) : BoundUserInterfaceMessage
{
    public string BountyId = bountyId;
}
