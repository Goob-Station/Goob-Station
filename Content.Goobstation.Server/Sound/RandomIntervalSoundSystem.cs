using Content.Goobstation.Shared.Sound.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Sound;

/// <summary>
/// periodically plays the sound from RandomIntervalSoundComponent (server-only).
/// </summary>
public sealed class RandomIntervalSoundSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomIntervalSoundComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<RandomIntervalSoundComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.NextSound == TimeSpan.Zero)
            ent.Comp.NextSound = _timing.CurTime + GetInterval(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<RandomIntervalSoundComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            // Do not update paused entities.
            if (Paused(uid))
                continue;

            if (comp.NextSound != TimeSpan.Zero && now < comp.NextSound)
                continue;

            var canPlay = true;
            EntityUid? owner = null;

            if (comp.RequireEquipped)
            {
                if (!TryComp(uid, out ClothingComponent? clothing) || clothing.InSlotFlag == null)
                    canPlay = false;

                else if (_containers.TryGetContainingContainer(uid, out var container))
                    owner = container.Owner;
            }

            if (comp.RequireAlive)
            {
                if (owner == null && _containers.TryGetContainingContainer(uid, out var container))
                    owner = container.Owner;

                if (owner == null || !_mobState.IsAlive(owner.Value))
                    canPlay = false;
            }

            if (canPlay)
                _audio.PlayPvs(comp.Sound, uid);

            comp.NextSound = now + GetInterval(comp);
        }
    }

    private TimeSpan GetInterval(RandomIntervalSoundComponent comp)
    {
        return comp.MinInterval < comp.MaxInterval
            ? _random.Next(comp.MinInterval, comp.MaxInterval)
            : comp.MaxInterval;
    }
}
