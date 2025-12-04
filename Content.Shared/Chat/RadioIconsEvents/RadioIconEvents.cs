using Content.Shared.Inventory;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat.RadioIconsEvents;

/// <summary>
///     Raised whenever a radio message is sent, contains the job icon and name of the sender, Added by goobstation
/// </summary>
public sealed class TransformSpeakerJobIconEvent(EntityUid sender, ProtoId<JobIconPrototype> jobIcon, string? jobName)
    : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;
    public EntityUid Sender = sender;
    public ProtoId<JobIconPrototype> JobIcon = jobIcon;
    public string? JobName = jobName;
}
