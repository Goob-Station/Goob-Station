using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Server.Chat.Systems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
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
        [Dependency] private readonly ISharedAdminLogManager _admin = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MassWhisperComponent, MassWhisperEvent>(OnMassWhisper);
        }

        private void OnMassWhisper(Entity<MassWhisperComponent> ent, ref MassWhisperEvent args)
        {
            if (args.Handled)
                return;

            var (uid, comp) = ent;

            // Broadcast station-wide announcement
            _chatSystem.DispatchStationAnnouncement(uid, Loc.GetString("hastur-announcement"), null, false, null, Color.FromHex("#f3ce6d"));

            _audio.PlayGlobal(comp.Sound, Filter.Broadcast(), true);

            // Apply EntropicPlumeAffectedComponent to all mobs on station
            var query = EntityQueryEnumerator<MobStateComponent>();
            while (query.MoveNext(out var mob))
            {
                if (mob.Owner == uid)
                    continue;

                var affected = EnsureComp<EntropicPlumeAffectedComponent>(mob.Owner);
                affected.Duration = comp.Duration;
            }
            _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent.Owner)} used Mass Whisper as a Hastur, affecting all entities on station.");

            args.Handled = true;
        }

    }
}
