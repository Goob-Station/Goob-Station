// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Explosion.Components;
using Content.Server.Administration.Logs;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Speech;
using Content.Shared.Access.Systems;
using Content.Shared.Database;
using Content.Shared.Hands;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Explosion.EntitySystems
{
    public sealed class VoiceTriggerSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly TriggerSystem _triggerSystem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<TriggerOnVoiceIdLockedComponent, GotEquippedHandEvent>(OnEquipped);
            SubscribeLocalEvent<TriggerOnVoiceIdLockedComponent, GotUnequippedHandEvent>(OnUnequipped);

            SubscribeLocalEvent<TriggerOnVoiceIdLockedComponent, ListenEvent>(OnListen);
        }

        private void OnEquipped(Entity<TriggerOnVoiceIdLockedComponent> ent, ref GotEquippedHandEvent args)
        {
            ent.Comp.User = args.User;
        }

        private void OnUnequipped(Entity<TriggerOnVoiceIdLockedComponent> ent, ref GotUnequippedHandEvent args)
        {
            ent.Comp.User = null;
        }

        private void OnListen(Entity<TriggerOnVoiceIdLockedComponent> ent, ref ListenEvent args)
        {
            if (ent.Comp.NextActivationTime > _timing.CurTime)
                return;

            if (ent.Comp.HolderOnly && args.Source != ent.Comp.User)
                return;

            var message = args.Message.Trim();

            if (!message.Contains(Loc.GetString(ent.Comp.KeyPhrase), StringComparison.InvariantCultureIgnoreCase)
            || _accessReader.IsAllowed(args.Source, ent ))
                return;

            _adminLogger.Add(LogType.Trigger, LogImpact.High,
                $"An ID locked voice-trigger on {ToPrettyString(ent):entity} was triggered by {ToPrettyString(args.Source):speaker} speaking the key-phrase {ent.Comp.KeyPhrase}.");

            _triggerSystem.Trigger(ent, ent);

            ent.Comp.NextActivationTime = _timing.CurTime + ent.Comp.ActivationCooldown;
        }
    }
}
