using Content.Goobstation.Common.Speech;
using Content.Goobstation.Shared.Loudspeaker.Components;
using Content.Goobstation.Shared.Loudspeaker.Events;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Loudspeaker.Systems;

public sealed class LoudSpeakerSystem : EntitySystem
{

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {

        base.Initialize();

        SubscribeLocalEvent<LoudspeakerComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<LoudspeakerComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<LoudspeakerComponent, GotEquippedHandEvent>(OnEquippedHands);
        SubscribeLocalEvent<LoudspeakerComponent, GotUnequippedHandEvent>(OnUnequippedHands);

        SubscribeLocalEvent<LoudspeakerHolderComponent, GetLoudspeakerEvent>(GetLoudSpeakers);
        SubscribeLocalEvent<LoudspeakerComponent, GetLoudspeakerDataEvent>(OnGetLoudspeakerData);
        SubscribeLocalEvent<LoudspeakerHolderComponent, GetSpeechSoundEvent>(OnGetSpeechSound);

        SubscribeLocalEvent<LoudspeakerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<LoudspeakerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

    }

    private void OnEquipped(EntityUid uid, LoudspeakerComponent comp, GotEquippedEvent args)
    {
        if (!args.SlotFlags.HasFlag(comp.RequiredSlot))
            return;

        EnsureComp<LoudspeakerHolderComponent>(args.Equipee).Loudspeakers.Add(uid);
    }

    private void OnUnequipped(EntityUid uid, LoudspeakerComponent comp, GotUnequippedEvent args)
    {
        if (!TryComp<LoudspeakerHolderComponent>(args.Equipee, out var holder))
            return;

        holder.Loudspeakers.Remove(uid);

        DoRemovalCheck(args.Equipee, holder);
    }

    private void OnEquippedHands(EntityUid uid, LoudspeakerComponent comp, GotEquippedHandEvent args)
    {
        if (!comp.WorksInHand)
            return;

        EnsureComp<LoudspeakerHolderComponent>(args.User).Loudspeakers.Add(uid);
    }

    private void OnUnequippedHands(EntityUid uid, LoudspeakerComponent comp, GotUnequippedHandEvent args)
    {
        if (!TryComp<LoudspeakerHolderComponent>(args.User, out var holder))
            return;

        holder.Loudspeakers.Remove(uid);

        DoRemovalCheck(args.User, holder);
    }

    private void GetLoudSpeakers(Entity<LoudspeakerHolderComponent> ent, ref GetLoudspeakerEvent args)
    {
        args.Loudspeakers = ent.Comp.Loudspeakers;
    }

    private void OnGetLoudspeakerData(Entity<LoudspeakerComponent> ent, ref GetLoudspeakerDataEvent args)
    {
        args.IsActive = ent.Comp.IsActive;

        args.FontSize = ent.Comp.FontSize;
        args.AffectRadio = ent.Comp.AffectRadio;
        args.AffectChat = ent.Comp.AffectChat;
        args.SpeechSounds = ent.Comp.SpeechSounds;
    }

    private void OnGetSpeechSound(Entity<LoudspeakerHolderComponent> ent, ref GetSpeechSoundEvent args)
    {

        foreach (var loudspeaker in ent.Comp.Loudspeakers)
        {
            var speechEv = new GetLoudspeakerDataEvent();
            RaiseLocalEvent(loudspeaker, ref speechEv);

            if (speechEv.SpeechSounds != null)
            {
                args.SpeechSoundProtoId = speechEv.SpeechSounds;
                return;
            }

        }
    }

    private void OnExamined(Entity<LoudspeakerComponent> ent, ref ExaminedEvent args)
    {
        var state = ent.Comp.IsActive ? "on" : "off";

        var message = ent.Comp.CanToggle
            ? Loc.GetString("loudspeaker-examine-toggleable", ("state", state))
            : Loc.GetString("loudspeaker-examine-generic");

        args.PushMarkup(message);
    }

    private void OnGetVerbs(Entity<LoudspeakerComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract
            || !ent.Comp.CanToggle)
            return;

        var user = args.User;

        AlternativeVerb toggleLoudspeakerVerb = new()
        {
            Act = () =>
            {
                ToggleLoudspeakerEffect(user, ent);
                ent.Comp.IsActive = !ent.Comp.IsActive;
                Dirty(ent);
            },
            Text = Loc.GetString("loudspeaker-toggle"),
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Effects/text.rsi"), "exclamation"),
        };

        args.Verbs.Add(toggleLoudspeakerVerb);
    }

    #region Helper methods

    private void DoRemovalCheck(EntityUid equipee, LoudspeakerHolderComponent comp)
    {
        if (comp.Loudspeakers.Count == 0) // only remove when theres no loudspeakers
        {
            RemComp<LoudspeakerHolderComponent>(equipee);
            return;
        }
    }

    private void ToggleLoudspeakerEffect(EntityUid user, Entity<LoudspeakerComponent> loudspeaker)
    {
        var state = !loudspeaker.Comp.IsActive ? "on" : "off";

        _audio.PlayPredicted(loudspeaker.Comp.ToggleSound, user, user);
        _popup.PopupClient(Loc.GetString("loudspeaker-toggle-popup", ("state", state)), user, user);
    }

    #endregion
}
