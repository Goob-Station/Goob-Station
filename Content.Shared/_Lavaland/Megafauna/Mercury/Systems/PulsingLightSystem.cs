using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// This system handles increasing the glow of an entity and lowering it on a loop. Additionally, plays a sound when the loop begins.
/// </summary>
public sealed class PulsingLightSystem : EntitySystem
{

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // This iterates over all entities that have this component.
        var query = EntityQueryEnumerator<PulsingLightComponent>();

        // this uses UID, comp because we want both the entity and the data
        while (query.MoveNext(out var uid, out var comp))
        {
            // if it's not time yet, skip the entity.
            if (_timing.CurTime < comp.NextUpdate)
                continue;

            // schedules the next time the logic will run
            comp.NextUpdate = _timing.CurTime + comp.Interval;

            // passes both uid and comp because it needs both the entity and the component data.
            OnBootUp((uid, comp));
        }
    }

    private void OnBootUp(Entity<PulsingLightComponent> ent)
    {
        // deconstructs tuple, which is a set of unchangeable elements, into readable variables. 
        var (uid, comp) = ent;

        if (!comp.SoundPlayed && comp.ShouldPlaySound)
        {
            _audio.PlayPredicted(comp.BootUpSound, uid, uid, null);
            comp.SoundPlayed = true;
        }

        // Ensures the entity has a light component and creates one if missing.
        var light = _lights.EnsureLight(comp.Owner);
        _lights.SetColor(comp.Owner, comp.LightColor, light);

        // If it has hit the cap, then start going back towards 0 glow.
        if (comp.ReduceGlow)
        {
            _lights.SetEnergy(comp.Owner, comp.CurrentGlow - comp.IncreaseBy, light);
            _lights.SetRadius(comp.Owner, comp.CurrentGlow - comp.IncreaseBy, light);
            comp.CurrentGlow -= comp.IncreaseBy;

            if (comp.CurrentGlow <= 0)
            {
                comp.CurrentGlow = 0;
                comp.ReduceGlow = false;
                comp.SoundPlayed = false;
            }
        }
        // If the glow hasn't reached the cap yet keep raising the glow
        else
        {
            _lights.SetEnergy(comp.Owner, comp.CurrentGlow + comp.IncreaseBy, light);
            _lights.SetRadius(comp.Owner, comp.CurrentGlow + comp.IncreaseBy, light);
            comp.CurrentGlow += comp.IncreaseBy;

            if (comp.CurrentGlow >= comp.GlowIntensity)
            {
                comp.CurrentGlow = comp.GlowIntensity;
                comp.ReduceGlow = true;
            }
        }

        Dirty(uid, comp);

    }
}
