using System.Diagnostics;
using System.Linq;
using System.Threading;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Goobstation.Shared.NTR;
using Content.Goobstation.Shared.NTR.Events;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Station;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

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
        if (args.Cancelled || !TryComp<PaperComponent>(args.Item, out var _))
            return;

        if (!HasValidStamps(args.Item))
        {
            args.Cancelled = true;
            if (_net.IsServer)
            {
                _popup.PopupEntity(Loc.GetString("ntr-console-insert-deny"), uid);
                _audio.PlayPvs(component.DenySound, uid);
            }
            return;
        }

        if (_net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("ntr-console-insert-accept"), uid);
            _audio.PlayPvs(component.SkipSound, uid);
            if (!TryComp<RandomDocumentComponent>(args.Item, out var documentComp))
                return;
            foreach (var task in documentComp.Tasks)
            {
                if (!_prototypeManager.TryIndex(task, out NtrTaskPrototype? taskProto))
                    return;
                if (args.User != null)
                    RaiseLocalEvent(uid, new TaskCompletedEvent(taskProto, args.User.Value));
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
        foreach (var task in documentComp.Tasks)
        {
            if (!_prototypeManager.TryIndex(task, out NtrTaskPrototype? taskProto) || taskProto == null)
                return new HashSet<string>();

            var entries = taskProto.Entries;
            if (entries.Count == 0)
                return new HashSet<string>();
            foreach (var entry in entries)
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

        var actualStamps = paperComp.StampedBy.Select(stamp => stamp.StampedName).ToList();
        return requiredStamps.All(actualStamps.Contains);
    }
}
