using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.StationRecords;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This handles...
/// </summary>
public sealed class BrigLockerSystem : EntitySystem
{

    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BrigLockerComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }

    private void OnInteract(EntityUid uid, BrigLockerComponent comp, EntityUid user)
    {
        if (!TryComp<AccessReaderComponent>(uid, out var accessReaderComponent))
            return;
        
        var prisonerId = GetPrisonerId(uid, comp, user);
        // use comp.Accessed now
        if (prisonerId == default && comp.Assigned == false)
        {
            _popupSystem.PopupClient(Loc.GetString("brig-locker-id-popup"), uid, user); // localize
            _audioSystem.PlayPredicted(comp.DenySound, uid, user);
             return;
        }

        if (!_accessReaderSystem.IsAllowed(user, uid))
        {
            _popupSystem.PopupClient(Loc.GetString("brig-locker-personnel-popup"), uid, user); // localize
            _audioSystem.PlayPredicted(comp.DenySound, uid, user);
            return;
        }

        if (prisonerId == default)

        {
            _popupSystem.PopupClient(Loc.GetString("brig-locker-unassign-popup"), uid, user); // localize
            // add error noise
            accessReaderComponent.AccessKeys.Clear();
            return;
        }

        if (!TryComp<StationRecordKeyStorageComponent>(prisonerId, out var stationRecordKeyStorageComponent))
            return;

        if (stationRecordKeyStorageComponent.Key is not { } stationRecordKey)
            return;

        accessReaderComponent.AccessKeys.Add(stationRecordKey);

        _popupSystem.PopupClient(Loc.GetString("brig-locker-assign-popup"), uid, user); // localize
        comp.Assigned = true;
    }

    private EntityUid GetPrisonerId(EntityUid uid, BrigLockerComponent comp, EntityUid user)
    {
        using var handsEnumerator = _handsSystem.EnumerateHands(user).GetEnumerator();

        while (handsEnumerator.MoveNext())
        {
            var hand = handsEnumerator.Current;
            if (hand.Container == null)
                continue;

            var handsUid = hand.Container.ContainedEntity;
            if (!TryComp<MetaDataComponent>(handsUid, out var metaData))
                continue;

            if (handsUid is not {} entityUid)
                continue;

            if (metaData?.EntityPrototype?.ID == comp.Check)
                return entityUid;
        }

        return default;
    }

    private void OnGetVerbs(EntityUid uid, BrigLockerComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        args.Verbs.Add(new InteractionVerb()
        {
            Text = Loc.GetString(component.Assigned ? "toggle-assign-verb-unassign" : "toggle-assign-verb-assign"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/character.svg.192dpi.png")),//Loc.GetString("loc-name"),Text =
            Act = () =>
            {
                OnInteract(uid, component, args.User);
            }
        });
    }
}
