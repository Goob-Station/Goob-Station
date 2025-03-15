using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared._Goobstation.NTR;

namespace Content.Shared._Goobstation.NTR;

[RegisterComponent]
public sealed partial class NTRBountyConsoleComponent : Component
{
    /// <summary>
    /// The id of the label entity spawned by the print label button.
    /// </summary>
    [DataField("bountyLabelId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BountyLabelId = "PaperCargoBountyManifest"; // todo

    /// <summary>
    /// The time at which the console will be able to print a label again.
    /// </summary>
    [DataField("nextPrintTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextPrintTime = TimeSpan.Zero;

    /// <summary>
    /// The time between prints.
    /// </summary>
    [DataField("printDelay")]
    public TimeSpan PrintDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The sound made when printing occurs
    /// </summary>
    [DataField("printSound")]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");

    /// <summary>
    /// The sound made when the bounty is completed.
    /// </summary>
    [DataField("completionSound")]
    public SoundSpecifier CompletionSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// The sound made when bounty completion is denied.
    /// </summary>
    [DataField("denySound")]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_two.ogg");
}

[NetSerializable, Serializable]
public sealed class NTRBountyConsoleState : BoundUserInterfaceState
{
    public List<NTRBountyData> Bounties;
    public List<NTRBountyHistoryData> History;
    public TimeSpan UntilNextUpdate;

    public NTRBountyConsoleState(List<NTRBountyData> bounties, List<NTRBountyHistoryData> history, TimeSpan untilNextUpdate)
    {
        Bounties = bounties;
        History = history;
        UntilNextUpdate = untilNextUpdate;
    }
}

[Serializable, NetSerializable]
public sealed class BountyClaimMessage : BoundUserInterfaceMessage
{
    public string BountyId;

    public BountyClaimMessage(string bountyId)
    {
        BountyId = bountyId;
    }
}

[Serializable, NetSerializable]
public sealed class BountyUpdateMessage : BoundUserInterfaceMessage
{
    public string BountyId;

    public BountyUpdateMessage(string bountyId)
    {
        BountyId = bountyId;
    }
}
