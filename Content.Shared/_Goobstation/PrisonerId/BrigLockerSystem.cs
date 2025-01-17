using Content.Shared.Access.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.StationRecords;
using Content.Shared.Verbs;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This handles...
/// </summary>
public sealed class BrigLockerSystem : EntitySystem
{

    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly EntityManager _audioSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BrigLockerComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }

    private void OnInteract(EntityUid uid, BrigLockerComponent comp, EntityUid user)
    {
        using var handsEnumerator = _handsSystem.EnumerateHands(user).GetEnumerator();
        EntityUid prisonerId = default;
        if (!TryComp<AccessReaderComponent>(uid, out var accessReaderComponent))
            return;
        while (handsEnumerator.MoveNext())
        {
            var hand = handsEnumerator.Current;
            if (hand.Container == null)
                continue;

            var handsUid = hand.Container.ContainedEntity;
            if (!TryComp<MetaDataComponent>(handsUid, out var metaData))
                continue;

            if (metaData?.EntityPrototype?.ID == "PrisonerID") // unhardcode
                prisonerId = (EntityUid) handsUid!;
        }

        // use comp.Accessed now

        if (prisonerId == default)
        {
            _popupSystem.PopupClient("Make sure your holding a prisoner ID.", user); // localize
            return;
        }

        if (!TryComp<StationRecordKeyStorageComponent>(prisonerId, out var stationRecordKeyStorageComponent))
            return;

        if (accessReaderComponent.AccessKeys.Count > 0)
        {
            _popupSystem.PopupClient("Unassigned the locker", user); // localize
            accessReaderComponent.AccessKeys.Clear();
            return;
        }

        accessReaderComponent.AccessKeys.Add((StationRecordKey) stationRecordKeyStorageComponent.Key!);
        _popupSystem.PopupClient("Assigned the locker", user); // localize

    }

    private void OnGetVerbs(EntityUid uid, BrigLockerComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        args.Verbs.Add(new InteractionVerb()
        {
            Text = "Assign", //Loc.GetString("loc-name"),Text = Loc.GetString(component.Locked ? "toggle-lock-verb-unlock" : "toggle-lock-verb-lock"),
            Act = () =>
            {
                OnInteract(uid, component, args.User);
            }
        });
    }
}
