// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Explosion.Components;
using Content.Server.Administration.Logs;
using Content.Server.Speech;
using Content.Server.Speech.Components;
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
            SubscribeLocalEvent<TriggerOnVoiceIdLockedComponent, MapInitEvent>(OnVoiceInit);
            SubscribeLocalEvent<TriggerOnVoiceIdLockedComponent, ListenEvent>(OnListen);
        }

        private void OnVoiceInit(EntityUid uid, TriggerOnVoiceIdLockedComponent comp, MapInitEvent args)
        {
            // Set the access levels.
            EnsureComp<AccessReaderComponent>(uid, out var accessReader);
            // Allow the item to listen.
            EnsureComp<ActiveListenerComponent>(uid).Range = comp.ListenRange;
            _accessReader.SetAccesses(uid, accessReader, comp.AccessLists);

        }

        private void OnListen(Entity<TriggerOnVoiceIdLockedComponent> ent, ref ListenEvent args)
        {
            var comp = ent.Comp;
            var message = args.Message.Trim();

            if (!string.IsNullOrWhiteSpace(comp.KeyPhrase) && message.Contains(comp.KeyPhrase, StringComparison.InvariantCultureIgnoreCase))
            {
                if (_accessReader.IsAllowed(args.Source, comp.Owner ))
                    return;

                _adminLogger.Add(LogType.Trigger, LogImpact.High,
                        $"An ID locked voice-trigger on {ToPrettyString(ent):entity} was triggered by {ToPrettyString(args.Source):speaker} speaking the key-phrase {comp.KeyPhrase}.");

                _triggerSystem.Trigger(ent, comp.Owner);
            }
        }
    }
}
