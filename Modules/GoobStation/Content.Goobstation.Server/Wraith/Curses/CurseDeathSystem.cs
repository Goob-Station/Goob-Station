using Content.Goobstation.Shared.Wraith.Curses;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Server.Body.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Wraith.Curses;

/// <summary>
/// This handles the custom logic of Curse of Death
/// </summary>
public sealed class CurseDeathSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SmokeSystem _smokeSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly WraithPointsSystem _wraithPoints = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseDeathComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<CurseDeathComponent, CurseEffectAppliedEvent>(OnCurseEffectApplied);
    }

    private void OnMobStateChanged(Entity<CurseDeathComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Critical || !ent.Comp.EndIsNigh)
        {
            _audio.PlayEntity(ent.Comp.CurseSound2, ent.Owner, ent.Owner);
            ent.Comp.EndIsNigh = true;
        }

        if (args.NewMobState != MobState.Dead)
            return;

        var xform = Transform(ent.Owner);
        var worldPos = _transformSystem.GetMapCoordinates(ent.Owner, xform);

        var solution = new Solution(ent.Comp.Reagent, 20);
        var smokeEnt = Spawn("Smoke", worldPos);
        _smokeSystem.StartSmoke(smokeEnt, solution, ent.Comp.SmokeDuration, ent.Comp.SmokeSpread);

        if (!TryComp<CurseHolderComponent>(ent.Owner, out var curseHolder))
            return;

        if (curseHolder.Curser != null)
            _wraithPoints.AdjustWpGenerationRate(ent.Comp.WpGeneration, curseHolder.Curser.Value); // wraith gets extra regen rate

        _bodySystem.GibBody(ent.Owner);
        RemCompDeferred<CurseHolderComponent>(ent.Owner);
    }

    private void OnCurseEffectApplied(Entity<CurseDeathComponent> ent, ref CurseEffectAppliedEvent args)
    {
        if (args.Curse != ent.Comp.Curse)
            return;

        ent.Comp.TicksElapsed++;

        // Calculate how much to scale the base damage
        var scale = ent.Comp.TicksElapsed * ent.Comp.RampMultiplier;

        // Create a scaled DamageSpecifier
        var scaledDamage = ent.Comp.BaseDamage * scale;

        if (!ent.Comp.MusicIsPlaying)
        {
            _audio.PlayEntity(ent.Comp.CurseSound1, ent.Owner, ent.Owner);
            ent.Comp.MusicIsPlaying = true;
        }

        // Apply the scaled damage instead of the fixed Damage field
        _damageableSystem.TryChangeDamage(ent.Owner, scaledDamage, targetPart: TargetBodyPart.All);
    }
}
