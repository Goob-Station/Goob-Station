using System.Linq;
using Content.Goobstation.Common.Style;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Style
{
    public sealed class StyleSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<StyleCounterComponent, UpdateStyleEvent>(OnUpdateRank);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<StyleCounterComponent>();
            while (query.MoveNext(out var uid, out var style))
            {
                var oldPoints = style.CurrentPoints;
                style.CurrentPoints = Math.Max(0, style.CurrentPoints -
                                                  (style.BaseDecayPerSecond * style.CurrentMultiplier * frameTime));

                // Check if points crossed a rank threshold during decay
                if (oldPoints != style.CurrentPoints)
                {
                    RaiseLocalEvent(uid, new UpdateStyleEvent());

                    if (_net.IsServer)
                    {
                        RaiseNetworkEvent(new StyleHudUpdateEvent(
                                style.Rank,
                                style.CurrentMultiplier,
                                style.RecentEvents.ToList()),
                            Filter.Entities(uid));
                    }
                }

                if (style.RecentEvents.Count > 0 &&
                    _timing.CurTime - style.LastEventTime > style.TimeToClear)
                {
                    style.RecentEvents.RemoveAt(0);
                    RaiseLocalEvent(uid, new UpdateStyleEvent());

                    if (_net.IsServer)
                    {
                        RaiseNetworkEvent(new StyleHudUpdateEvent(
                                style.Rank,
                                style.CurrentMultiplier,
                                style.RecentEvents.ToList()),
                            Filter.Entities(uid));
                    }
                }
            }
        }
        public void AddStyleEvent(EntityUid? uid, string eventText, StyleCounterComponent? component = null, Color? color = null)
        {
            if (uid == null || !Resolve(uid.Value, ref component))
                return;

            // only like 5 same events to prevent spam
            var similarEvents = component.RecentEvents
                .Where(e => e.Contains(eventText))
                .Count();

            if (similarEvents >= 5 && // todo: maybe unhardcode this.
                _timing.CurTime - component.LastEventTime < TimeSpan.FromSeconds(1))
            {
                return;
            }
            if (component.RecentEvents.Count > 0 &&
                component.RecentEvents.Last().Contains(eventText) &&
                _timing.CurTime - component.LastEventTime < TimeSpan.FromSeconds(0.1))
            {
                return;
            }

            var formattedText = color.HasValue
                ? $"[color={color.Value.ToHex()}]{eventText}[/color]"
                : eventText;

            component.RecentEvents.Add(formattedText);
            component.LastEventTime = _timing.CurTime;

            RaiseLocalEvent(uid.Value, new UpdateStyleEvent());
            if (_net.IsServer)
            {
                RaiseNetworkEvent(new StyleHudUpdateEvent(
                        component.Rank,
                        component.CurrentMultiplier,
                        component.RecentEvents.ToList()),
                    Filter.Entities(uid.Value));
            }
        }

        private void OnUpdateRank(EntityUid uid, StyleCounterComponent style, UpdateStyleEvent args)
        {
            StyleRank newRank = StyleRank.F;
            var highestRank = StyleRank.F;
            float highestMultiplier = 1.0f;

            foreach (var rankProto in _proto.EnumeratePrototypes<StyleRankPrototype>())
            {
                if (Enum.TryParse<StyleRank>(rankProto.ID, out var rank) &&
                    style.CurrentPoints >= rankProto.PointsRequired &&
                    rank > highestRank)
                {
                    highestRank = rank;
                    newRank = rank;
                    highestMultiplier = rankProto.Multiplier;
                }
            }

            if (newRank != style.Rank)
            {
                style.Rank = newRank;
                style.CurrentMultiplier = highestMultiplier;

                if (_net.IsServer)
                {
                    RaiseNetworkEvent(new StyleHudUpdateEvent(
                            style.Rank,
                            style.CurrentMultiplier,
                            style.RecentEvents.ToList()),
                        Filter.Entities(uid));
                }
                Dirty(uid, style);
            }
        }
    }
}
