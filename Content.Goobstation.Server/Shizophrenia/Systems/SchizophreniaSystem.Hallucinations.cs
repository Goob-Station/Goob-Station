using Content.Goobstation.Common.Actions;
using Content.Goobstation.Common.Chat;
using Content.Goobstation.Common.Pointing;
using Content.Goobstation.Shared.Shizophrenia;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Speech;
using Content.Shared.StepTrigger.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Shizophrenia;

public sealed partial class SchizophreniaSystem : EntitySystem
{
    private void InitializeHallucinations()
    {
        SubscribeLocalEvent<HallucinationComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<HallucinationComponent, PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<HallucinationComponent, ActionAddedDirectEvent>(OnActionAdded);

        SubscribeLocalEvent<HallucinationComponent, MapInitEvent>(OnHallucinationInit);
        SubscribeLocalEvent<HallucinationComponent, ComponentShutdown>(OnHallucinationShutdown);

        SubscribeLocalEvent<HallucinationComponent, BeforeChatMessageSentEvent>(OnBeforeChatMessage);
        SubscribeLocalEvent<HallucinationComponent, OverrideEmoteSoundEvent>(OverrideEmoteSound);
        SubscribeLocalEvent<HallucinationComponent, SetupPointingArrowEvent>(OnSetupPointer);

        SubscribeLocalEvent<HallucinationComponent, AttemptMobCollideEvent>(OnMobCollision);
        SubscribeLocalEvent<HallucinationComponent, AttemptMobTargetCollideEvent>(OnMobCollisionTarget);
        SubscribeLocalEvent<HallucinationComponent, PreventCollideEvent>(OnPreventCollision);
        SubscribeLocalEvent<HallucinationComponent, InteractionAttemptEvent>(OnInteractionAttempt);
    }

    #region Pvs overrides
    private void OnPlayerAttached(Entity<HallucinationComponent> ent, ref PlayerAttachedEvent args)
    {
        if (!TryComp<SchizophreniaComponent>(ent.Comp.Ent, out var schizophrenia))
            return;

        foreach (var item in schizophrenia.Hallucinations)
            _pvsOverride.AddForceSend(item, args.Player);
    }

    private void OnPlayerDetached(Entity<HallucinationComponent> ent, ref PlayerDetachedEvent args)
    {
        if (!TryComp<SchizophreniaComponent>(ent.Comp.Ent, out var schizophrenia))
            return;

        foreach (var item in schizophrenia.Hallucinations)
            _pvsOverride.RemoveForceSend(item, args.Player);
    }
    private void OnActionAdded(Entity<HallucinationComponent> ent, ref ActionAddedDirectEvent args)
    {
        AddAsHallucination(ent.Comp.Ent, args.Action);

        if (_player.TryGetSessionByEntity(ent.Owner, out var ourSession))
            _pvsOverride.AddForceSend(args.Action, ourSession);
    }
    private void OnHallucinationInit(Entity<HallucinationComponent> ent, ref MapInitEvent args)
    {
        foreach (var action in _actions.GetActions(ent.Owner))
        {
            AddAsHallucination(ent.Comp.Ent, action);

            if (_player.TryGetSessionByEntity(ent.Owner, out var ourSession))
                _pvsOverride.AddForceSend(action, ourSession);
        }
    }

    private void OnHallucinationShutdown(Entity<HallucinationComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SchizophreniaComponent>(ent.Comp.Ent, out var schizophrenia))
            return;

        schizophrenia.Hallucinations.Remove(ent.Owner);

        if (_player.TryGetSessionByEntity(ent.Comp.Ent, out var session))
            _pvsOverride.RemoveForceSend(ent.Owner, session);

        // For sounds that are deleted really fast but need to be heard by hallucinations
        foreach (var item in schizophrenia.Hallucinations)
        {
            if (_player.TryGetSessionByEntity(item, out var hallucinationSession))
                _pvsOverride.RemoveForceSend(ent.Owner, hallucinationSession);
        }

        if (schizophrenia.Hallucinations.Count <= 0)
            RemComp(ent.Comp.Ent, schizophrenia);
    }
    #endregion

    private void OnBeforeChatMessage(Entity<HallucinationComponent> ent, ref BeforeChatMessageSentEvent args)
    {
        args.Cancel();

        var channel = (InGameICChatType) args.Channel;
        var range = channel == InGameICChatType.Whisper ? ChatSystem.WhisperMuffledRange : ChatSystem.VoiceRange;

        // Building our message
        var message = channel switch
        {
            InGameICChatType.Speak => _chat.WrapPublicMessage(ent.Owner, $"[color={ent.Comp.ChatColor.ToHex()}]{Name(ent.Owner)}[/color]", args.Message),
            InGameICChatType.Whisper => _chat.WrapWhisperMessage(ent.Owner, "chat-manager-entity-whisper-wrap-message", $"[color={ent.Comp.ChatColor.ToHex()}]{Name(ent.Owner)}[/color]", args.Message),
            InGameICChatType.Emote => Loc.GetString("chat-manager-entity-me-wrap-message",
            ("entityName", Name(ent.Owner)),
            ("entity", ent),
            ("message", FormattedMessage.RemoveMarkupOrThrow(args.Message))),
            _ => null
        };

        var chatChannel = channel switch
        {
            InGameICChatType.Speak => ChatChannel.Local,
            InGameICChatType.Whisper => ChatChannel.Whisper,
            InGameICChatType.Emote => ChatChannel.Emotes,
            _ => ChatChannel.Local
        };

        if (message == null)
            return;

        var filter = Filter.Empty();

        // Trying to get sessions and send message
        if (_player.TryGetSessionByEntity(ent.Owner, out var hallucinationSession))
        {
            _chatMan.ChatMessageToOne(chatChannel, args.Message, message, ent.Owner, false, hallucinationSession.Channel);
            filter.AddPlayer(hallucinationSession);
        }

        var xform = Transform(ent.Owner);
        var schizoXform = Transform(ent.Comp.Ent);

        // Check if we close enough for them to hear us
        if (xform.Coordinates.TryDistance(EntityManager, schizoXform.Coordinates, out var distance)
            && distance <= range
            && _player.TryGetSessionByEntity(ent.Comp.Ent, out var session))
        {
            _chatMan.ChatMessageToOne(chatChannel, args.Message, message, ent.Owner, false, session.Channel);
            filter.AddPlayer(session);
        }

        _chat.TryEmoteChatInput(ent.Owner, args.Message);

        // Down we have speech sounds handling
        if (!TryComp<SpeechComponent>(ent.Owner, out var speech) || speech.SpeechSounds == null || channel == InGameICChatType.Emote)
            return;

        var sound = _audio.PlayEntity(_speech.GetSpeechSound((ent.Owner, speech), args.Message), filter, ent.Owner, false);

        if (!sound.HasValue)
            return;

        foreach (var recipient in filter.Recipients)
        {
            _pvsOverride.AddForceSend(sound.Value.Entity, recipient);

            AddAsHallucination(ent.Comp.Ent, sound.Value.Entity, false);   // to avoid error spam
        }
    }

    private void OverrideEmoteSound(Entity<HallucinationComponent> ent, ref OverrideEmoteSoundEvent args)
    {
        var filter = Filter.Entities(ent.Owner, ent.Comp.Ent);
        var sound = _audio.PlayEntity(args.Sound, filter, ent.Owner, false);

        if (!sound.HasValue)
            return;

        foreach (var recipient in filter.Recipients)
        {
            _pvsOverride.AddForceSend(sound.Value.Entity, recipient);

            AddAsHallucination(ent.Comp.Ent, sound.Value.Entity, false);   // to avoid error spam
        }
    }

    private void OnSetupPointer(Entity<HallucinationComponent> ent, ref SetupPointingArrowEvent args)
    {
        if (_player.TryGetSessionByEntity(ent.Owner, out var hallucinationSession))
        {
            _pvsOverride.AddForceSend(args.Arrow, hallucinationSession);
        }

        AddAsHallucination(ent.Comp.Ent, args.Arrow, false);
    }

    #region Everything interaction-related
    private void OnMobCollision(Entity<HallucinationComponent> ent, ref AttemptMobCollideEvent args)
        => args.Cancelled = true;

    private void OnMobCollisionTarget(Entity<HallucinationComponent> ent, ref AttemptMobTargetCollideEvent args)
        => args.Cancelled = true;

    private void OnPreventCollision(Entity<HallucinationComponent> ent, ref PreventCollideEvent args)
    {
        if (HasComp<StepTriggerComponent>(args.OtherEntity))
            args.Cancelled = true;
    }

    private void OnInteractionAttempt(Entity<HallucinationComponent> ent, ref InteractionAttemptEvent args)
        => args.Cancelled = true;
    #endregion
}
