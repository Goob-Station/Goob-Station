using Content.Goobstation.Shared.IntrinsicVoiceModulator.VoiceMask;
using Content.Server.VoiceMask;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.IntrinsicVoiceModulator.VoiceMask;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class VoiceMaskSystemGoob : EntitySystem
{
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly VoiceMaskSystem _voicemask = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskChangeJobIconMessage>(OnChangeJobIcon); // Gaby Station -> Job icons
        SubscribeLocalEvent<VoiceMaskComponent, InventoryRelayedEvent<TransformSpeakerJobIconEvent>>(OnTransformJobIcon); // GabyStation -> Radio icons
    }

    private void OnTransformJobIcon(Entity<VoiceMaskComponent> ent, ref InventoryRelayedEvent<TransformSpeakerJobIconEvent> args)
    {
        if (ent.Comp.JobIconProtoId is { } jobIcon)
            args.Args.JobIcon = jobIcon;

        if (!string.IsNullOrWhiteSpace(ent.Comp.JobName))
            args.Args.JobName = ent.Comp.JobName;
    }

    private void OnChangeJobIcon(Entity<VoiceMaskComponent> entity, ref VoiceMaskChangeJobIconMessage ev)
    {
        if (!_proto.TryIndex(ev.JobIconProtoId, out var proto)
            || !proto.AllowSelection)
            return;

        entity.Comp.JobIconProtoId = proto.ID;

        entity.Comp.JobName = _job.TryFindJobFromIcon(proto, out var job) ? job.LocalizedName : null;

        _popupSystem.PopupEntity(Loc.GetString("voice-mask-popup-success"), entity, ev.Actor);
        _voicemask.UpdateUI(entity);
    }
}
