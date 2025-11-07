using Content.Goobstation.Server.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// handles the becoming a werewolf by encountering one idk
/// </summary>
public sealed class WerewolfBitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _gambling = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfBitComponent, ComponentStartup>(OnStart);
    }

    public void OnStart(EntityUid uid, WerewolfBitComponent comp, ComponentStartup args)
    {
        comp.StartTime = _timing.CurTime;
        comp.WillBeSixtyFive = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WerewolfBitComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.StartTime == null)
                continue;

            var curTime = _timing.CurTime;
            var timePassed = curTime - component.StartTime.Value;
        }
    }

    private void Start65ing(EntityUid uid, WerewolfBitComponent component)
    {
        component.WillBeSixtyFive = true;
        var random = new Random();
        if (random.NextDouble() < component.SixtyFiveChance)
        {
            RemComp<WerewolfBitComponent>(uid); // todo figure out ts
        }
    }
}
