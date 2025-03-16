using Content.Server._Goobstation.Explosion.Components;
using Content.Server.Administration.Logs;
using Content.Server.Speech;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Database;

namespace Content.Server.Explosion.EntitySystems
{
    public sealed partial class VoiceTriggerSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly TriggerSystem _triggerSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<TriggerOnVoiceIdLockedComponent, ComponentInit>(OnVoiceInit);
            SubscribeLocalEvent<TriggerOnVoiceIdLockedComponent, ListenEvent>(OnListen);
        }

        private void OnVoiceInit(EntityUid uid, TriggerOnVoiceIdLockedComponent comp, ComponentInit args)
        {
            // Set the access levels.
            EnsureComp<AccessReaderComponent>(uid, out var accessReader);
            _accessReader.SetAccesses(uid, accessReader, comp.AccessLists);

        }

        private void OnListen(Entity<TriggerOnVoiceIdLockedComponent> ent, ref ListenEvent args)
        {
            var comp = ent.Comp;
            var message = args.Message.Trim();

            if (!string.IsNullOrWhiteSpace(comp.KeyPhrase) && message.Contains(comp.KeyPhrase, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!_accessReader.IsAllowed(args.Source, comp.Owner ))
                    return;

                _adminLogger.Add(LogType.Trigger, LogImpact.High,
                        $"An ID locked voice-trigger on {ToPrettyString(ent):entity} was triggered by {ToPrettyString(args.Source):speaker} speaking the key-phrase {comp.KeyPhrase}.");

                _triggerSystem.Trigger(ent, args.Source);
            }
        }
    }
}
