using System.Linq;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Goobstation.Shared.NTR.Documents;
using Content.Goobstation.Shared.NTR.Events;
using Content.Shared.Containers.ItemSlots;

namespace Content.Goobstation.Shared.NTR;

public sealed class SharedNtrTaskSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NtrTaskProviderComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt);
    }
    private void OnInsertAttempt(EntityUid uid,
        NtrTaskProviderComponent component,
        ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled || !TryComp<PaperComponent>(args.Item, out _))
            return;

        if (HasComp<SpamDocumentComponent>(args.Item))
        {
            args.Cancelled = true;
            if (_net.IsServer && args.User != null)
            {
                var ev = new TaskFailedEvent(args.User.Value);
                RaiseLocalEvent(uid, ev);
            }
            return;
        }
        if (!HasValidStamps(args.Item))
        {
            args.Cancelled = true;
            if (_net.IsServer)
            {
                if (args.User != null)
                {
                    _popup.PopupEntity(Loc.GetString("ntr-console-insert-deny"), uid, args.User.Value);
                }
                _audio.PlayPvs(component.DenySound, uid);
            }
            return;
        }

        if (_net.IsServer)
        {
            if (args.User != null)
            {
                _popup.PopupEntity(Loc.GetString("ntr-console-insert-accept"), uid, args.User.Value);
            }
            _audio.PlayPvs(component.SkipSound, uid);
            if (!TryComp<RandomDocumentComponent>(args.Item, out var documentComp))
                return;

            foreach (var taskId in documentComp.Tasks)
            {
                if (!_prototypeManager.TryIndex(taskId, out NtrTaskPrototype? taskProto))
                    continue;

                var ev = new TaskCompletedEvent(taskProto);
                RaiseLocalEvent(uid, ev);
            }
            _entityManager.QueueDeleteEntity(args.Item);
        }
    }

    private bool HasValidStamps(EntityUid paper)
    {
        if (!TryComp<PaperComponent>(paper, out var paperComp) ||
            !TryComp<RandomDocumentComponent>(paper, out var documentComp))
            return false;

        var requiredStamps = GetRequiredStamps(documentComp);
        if (requiredStamps.Count == 0)
            return false;

        return AreStampsCorrect(paperComp, requiredStamps);
    }

    private HashSet<string> GetRequiredStamps(RandomDocumentComponent documentComp)
    {
        var requiredStamps = new HashSet<string>();
        foreach (var taskId in documentComp.Tasks)
        {
            if (!_prototypeManager.TryIndex(taskId, out NtrTaskPrototype? taskProto) || taskProto == null)
                continue;

            foreach (var entry in taskProto.Entries)
            {
                foreach (var stamp in entry.Stamps)
                {
                    requiredStamps.Add(stamp);
                }
            }
        }
        return requiredStamps;
    }

    private bool AreStampsCorrect(PaperComponent paperComp, HashSet<string> requiredStamps)
    {
        if (paperComp.StampedBy.Count == 0)
            return false;

        var actualStamps = paperComp.StampedBy
            .Select(stamp => stamp.StampedName)
            .ToList();

        return requiredStamps.All(actualStamps.Contains);
    }
}
