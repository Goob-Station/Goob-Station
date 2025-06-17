using Content.Goobstation.Common.Style;
using Content.Goobstation.Shared.Style;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Style;

public sealed class StyleEventSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly StyleSystem _styleSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<SkeletonAccentComponent, ScreamActionEvent>(OnAckAck);
    }

    private void OnAckAck(EntityUid uid, SkeletonAccentComponent component, ScreamActionEvent args)
    {
        if (!_gameTiming.IsFirstTimePredicted
            || !TryComp<StyleCounterComponent>(uid, out var counter))
            return;

        counter.CurrentPoints += 50; // just for the memes
        RaiseLocalEvent(uid, new UpdateStyleEvent()); // todo: delete this if i cant fix the updates
        _styleSystem.AddStyleEvent(uid, "+ACK ACK", counter, Color.AntiqueWhite);
    }
}
