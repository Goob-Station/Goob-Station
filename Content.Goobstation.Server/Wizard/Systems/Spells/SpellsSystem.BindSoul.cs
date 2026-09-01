using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Gibbing.Events;
using Content.Shared.Magic.Components;
using Content.Shared.Mind;
using Content.Shared.Speech.Components;

namespace Content.Goobstation.Server.Wizard.Spells.Systems;

public sealed partial class SpellsSystem
{
    private LocId _locMsgLichGreeting = "lich-greeting";

    protected override void BindSoulRelay(BindSoulEvent ev, EntityUid oldEnt, EntityUid newEntity, MindComponent mindComponent)
    {
        _serverInventory.TransferEntityInventories(oldEnt, newEntity);
        foreach (var hand in _hands.EnumerateHeld(oldEnt))
        {
            _hands.TryDrop(oldEnt, hand, checkActionBlocker: false);
            _hands.TryPickupAnyHand(newEntity, hand);
        }

        SetGear(newEntity, ev.Gear, false, false);

        if (TryComp(ev.Action.Owner, out SpeakOnActionComponent? speak))
        {
            DelayedSpeech(speak.Sentence == null ? null : Loc.GetString(speak.Sentence.Value),
                newEntity,
                oldEnt,
                MagicSchool.Necromancy);
        }

        _body.GibBody(oldEnt, contents: GibContentsOption.Gib);

        if (!_playerManager.TryGetSessionById(mindComponent.UserId, out var session))
            return;

        _antag.SendBriefing(session, _locMsgLichGreeting, Color.DarkRed, ev.Sound);
    }
}