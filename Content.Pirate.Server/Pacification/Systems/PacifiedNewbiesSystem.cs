using Content.Pirate.Server.Pacification.Managers;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.CombatMode.Pacification;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;

namespace Content.Pirate.Server.Pacification.Systems
{
    public sealed class PacifiedNewbiesSystem : EntitySystem
    {
        [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        private static readonly PacifyManager _pacifyManager = new();

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        }

        private void OnPlayerAttached(PlayerAttachedEvent ev)
        {
            if (!_playerManager.TryGetSessionById(ev.Player.UserId, out var playerSession))
                return;

            var overallPlaytime = _playTimeTracking.GetOverallPlaytime(playerSession);
            if (overallPlaytime.TotalHours <= 1 || _pacifyManager.IsPacified(playerSession.Name))
            {
                EnsureComp<PacifiedComponent>(ev.Entity);
            }
        }
    }
}
