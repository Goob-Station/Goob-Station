using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Server.Chat.Systems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Hastur.Systems
{
    public sealed class MassWhisperSystem : EntitySystem
    {
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MassWhisperComponent, MassWhisperEvent>(OnMassWhisper);
        }

        private void OnMassWhisper(Entity<MassWhisperComponent> ent, ref MassWhisperEvent args)
        {
            if (args.Handled)
                return;

            var uid = ent.Owner;
            var comp = ent.Comp;

            // Broadcast station-wide announcement
            _chatSystem.DispatchStationAnnouncement(uid, Loc.GetString("hastur-announcement"), null, false, null, Color.FromHex("#f3ce6d"));

            // Play sound globally
            foreach (var player in EntityManager.EntityQuery<ActorComponent>())
            {
                _audio.PlayEntity(comp.Sound, player.Owner, player.Owner);
                _popup.PopupEntity(Loc.GetString("hastur-insanityaura-begin1"), player.Owner, player.Owner);
            }

            // Apply EntropicPlumeAffectedComponent to all mobs on station
            var allMobs = EntityManager.EntityQuery<MobStateComponent>();
            foreach (var mob in allMobs)
            {
                // Skip Hastur themselves
                if (mob.Owner == uid)
                    continue;

                // Optional filters: ignore ghosts, admins, certain factions, etc.
                if (!EntityManager.TryGetComponent(mob.Owner, out TransformComponent? xform))
                    continue;

                var affected = EnsureComp<EntropicPlumeAffectedComponent>(mob.Owner);
                affected.Duration = comp.Duration;
            }

            args.Handled = true;
        }

    }
}
