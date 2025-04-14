using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.NTR.Documents;
using Content.Goobstation.Shared.NTR.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.NTR;

public sealed class SharedNtrTaskSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NtrTaskConsoleComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt);
    }

    private void OnInsertAttempt(EntityUid uid,
        NtrTaskConsoleComponent component,
        ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled || !TryComp<PaperComponent>(args.Item, out _))
            return;

        if (HasComp<SpamDocumentComponent>(args.Item) || !HasValidStamps(args.Item))
        {
            args.Cancelled = true;
            if (_net.IsServer && args.User != null)
                RaiseLocalEvent(uid, new TaskFailedEvent(args.User.Value));
            return;
        }
        if (TryComp<RandomDocumentComponent>(args.Item, out var documentComp))
        {
            foreach (var taskId in documentComp.Tasks)
            {
                if (_prototypeManager.TryIndex(taskId, out NtrTaskPrototype? taskProto))
                {
                    var completeEv = new TaskCompletedEvent(taskProto);
                    RaiseLocalEvent(uid, completeEv);
                }
            }
        }
        if (_net.IsServer)
        {
            var ev = new DocumentInsertedEvent(args.Item, uid, args.User);
            RaiseLocalEvent(ev);
        }
    }

    private bool HasValidStamps(EntityUid paper)
    {
        if (!TryComp<PaperComponent>(paper, out var paperComp) ||
            !TryComp<RandomDocumentComponent>(paper, out var documentComp))
            return false;

        var requiredStamps = GetRequiredStamps(documentComp);
        return requiredStamps.Count != 0 && AreStampsCorrect(paperComp, requiredStamps);
    }

    private HashSet<string> GetRequiredStamps(RandomDocumentComponent documentComp)
    {
        // TODO: if has not required stamps, make task failed
        var requiredStamps = new HashSet<string>();
        foreach (var taskId in documentComp.Tasks)
        {
            if (!_prototypeManager.TryIndex(taskId, out NtrTaskPrototype? taskProto))
                continue;

            foreach (var entry in taskProto.Entries)
                requiredStamps.UnionWith(entry.Stamps);
        }
        return requiredStamps;
    }

    private bool AreStampsCorrect(PaperComponent paperComp, HashSet<string> requiredStamps)
    {
        if (paperComp.StampedBy.Count == 0 || requiredStamps.Count == 0)
            return false;

        foreach (var requiredStamp in requiredStamps)
        {
            bool found = false;
            foreach (var stamp in paperComp.StampedBy)
            {
                if (stamp.StampedName == requiredStamp)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                return false;
        }
        return true;
    }
}
public sealed class DocumentInsertedEvent : EntityEventArgs
{
    public EntityUid Document;
    public EntityUid Console;
    public EntityUid? User;

    public DocumentInsertedEvent(EntityUid document, EntityUid console, EntityUid? user)
    {
        Document = document;
        Console = console;
        User = user;
    }
}
