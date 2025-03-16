using Content.Server.Explosion.Components;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Server.Explosion.EntitySystems
{
    public sealed partial class TriggerSystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!; // Goobstation

        private void InitializeVoice()
        {
            SubscribeLocalEvent<TriggerOnVoiceComponent, ComponentInit>(OnVoiceInit);
            SubscribeLocalEvent<TriggerOnVoiceComponent, ExaminedEvent>(OnVoiceExamine);
            SubscribeLocalEvent<TriggerOnVoiceComponent, GetVerbsEvent<AlternativeVerb>>(OnVoiceGetAltVerbs);
            SubscribeLocalEvent<TriggerOnVoiceComponent, ListenEvent>(OnListen);
        }

        private void OnVoiceInit(EntityUid uid, TriggerOnVoiceComponent comp, ComponentInit args)
        {
            if (comp.IsListening)
                EnsureComp<ActiveListenerComponent>(uid).Range = comp.ListenRange;
            else
                RemCompDeferred<ActiveListenerComponent>(uid);

            // If the voice trigger is not restricted, do not apply a accessreader comp. - Goobstation Start
            if (!comp.RestrictById)
                return;

            // Set the access levels.
            EnsureComp<AccessReaderComponent>(uid, out var accessReader);
            _accessReader.SetAccesses(uid, accessReader, comp.AccessLists);
            // Goobstation - End
        }

        private void OnListen(Entity<TriggerOnVoiceComponent> ent, ref ListenEvent args)
        {
            var component = ent.Comp;
            var message = args.Message.Trim();

            if (component.IsRecording)
            {
                var ev = new ListenAttemptEvent(args.Source);
                RaiseLocalEvent(ent, ev);

                if (ev.Cancelled)
                    return;

                if (message.Length >= component.MinLength && message.Length <= component.MaxLength)
                    FinishRecording(ent, args.Source, args.Message);
                else if (message.Length > component.MaxLength)
                    _popupSystem.PopupEntity(Loc.GetString("popup-trigger-voice-record-failed-too-long"), ent);
                else if (message.Length < component.MinLength)
                    _popupSystem.PopupEntity(Loc.GetString("popup-trigger-voice-record-failed-too-short"), ent);

                return;
            }

            if (!string.IsNullOrWhiteSpace(component.KeyPhrase) && // Goobstation - Start
                message.Contains(component.KeyPhrase, StringComparison.InvariantCultureIgnoreCase) &&
                !component.RestrictById)
            {
                _adminLogger.Add(LogType.Trigger, LogImpact.High,
                        $"A voice-trigger on {ToPrettyString(ent):entity} was triggered by {ToPrettyString(args.Source):speaker} speaking the key-phrase {component.KeyPhrase}.");
                Trigger(ent, args.Source);
            }

            if (!string.IsNullOrWhiteSpace(component.KeyPhrase) &&
                message.Contains(component.KeyPhrase, StringComparison.InvariantCultureIgnoreCase) &&
                component.RestrictById)
            {
                if (!_accessReader.IsAllowed(args.Source, component.Owner))
                    Trigger(ent, args.Source);
            }
            // Goobstation - End
        }

        private void OnVoiceGetAltVerbs(Entity<TriggerOnVoiceComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            var component = ent.Comp;

            var @event = args;
            args.Verbs.Add(new AlternativeVerb()
            {
                Text = Loc.GetString(component.IsRecording ? "verb-trigger-voice-stop" : "verb-trigger-voice-record"),
                Act = () =>
                {
                    if (component.IsRecording)
                        StopRecording(ent);
                    else
                        StartRecording(ent, @event.User);
                },
                Priority = 1
            });

            if (string.IsNullOrWhiteSpace(component.KeyPhrase))
                return;

            args.Verbs.Add(new AlternativeVerb()
            {
                Text = Loc.GetString("verb-trigger-voice-clear"),
                Act = () =>
                {
                    component.KeyPhrase = null;
                    component.IsRecording = false;
                    RemComp<ActiveListenerComponent>(ent);
                }
            });
        }

        public void StartRecording(Entity<TriggerOnVoiceComponent> ent, EntityUid user)
        {
            var component = ent.Comp;
            component.IsRecording = true;
            EnsureComp<ActiveListenerComponent>(ent).Range = component.ListenRange;

            _adminLogger.Add(LogType.Trigger, LogImpact.Low,
                    $"A voice-trigger on {ToPrettyString(ent):entity} has started recording. User: {ToPrettyString(user):user}");

            _popupSystem.PopupEntity(Loc.GetString("popup-trigger-voice-start-recording"), ent);
        }

        public void StopRecording(Entity<TriggerOnVoiceComponent> ent)
        {
            var component = ent.Comp;
            component.IsRecording = false;
            if (string.IsNullOrWhiteSpace(component.KeyPhrase))
                RemComp<ActiveListenerComponent>(ent);

            _popupSystem.PopupEntity(Loc.GetString("popup-trigger-voice-stop-recording"), ent);
        }

        public void FinishRecording(Entity<TriggerOnVoiceComponent> ent, EntityUid source, string message)
        {
            var component = ent.Comp;
            component.KeyPhrase = message;
            component.IsRecording = false;

            _adminLogger.Add(LogType.Trigger, LogImpact.Low,
                    $"A voice-trigger on {ToPrettyString(ent):entity} has recorded a new keyphrase: '{component.KeyPhrase}'. Recorded from {ToPrettyString(source):speaker}");

            _popupSystem.PopupEntity(Loc.GetString("popup-trigger-voice-recorded", ("keyphrase", component.KeyPhrase!)), ent);
        }

        private void OnVoiceExamine(EntityUid uid, TriggerOnVoiceComponent component, ExaminedEvent args)
        {
            if (args.IsInDetailsRange)
            {
                args.PushText(string.IsNullOrWhiteSpace(component.KeyPhrase)
                    ? Loc.GetString("trigger-voice-uninitialized")
                    : Loc.GetString("examine-trigger-voice", ("keyphrase", component.KeyPhrase)));
            }
        }
    }
}
