using System.Linq;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Damage;

/// <summary>
///     We have to use it's own system even for the damage field because WIZDEN SYSTEMS FUCKING SUUUUUUUUUUUCKKKKKKKKKKKKKKK
/// </summary>
public abstract class SharedDamageSquareSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float ImmunityFrames = 0.3f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageSquareComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<DamageSquareComponent> ent, ref MapInitEvent args)
    {
        Timer.Spawn((int) ent.Comp.DamageDelay * 1000,
            () =>
            {
                if (!TerminatingOrDeleted(ent))
                    Damage(ent);
            });
    }

    private void Damage(Entity<DamageSquareComponent> field)
    {
        var xform = Transform(field);
        if (xform.GridUid == null)
            return;

        var grid = xform.GridUid.Value;
        var tile = _map.GetTileRef(grid, Comp<MapGridComponent>(grid), xform.Coordinates);

        var lookup = _lookup.GetLocalEntitiesIntersecting(tile, 0f, LookupFlags.Uncontained)
            .Where(HasComp<MobStateComponent>)
            .ToList();

        foreach (var entity in lookup)
        {
            if (!TryComp<DamageableComponent>(entity, out var dmg))
                continue;

            if (TryComp<DamageSquareImmunityComponent>(entity, out var immunity))
            {
                if (immunity.HasImmunityUntil > _timing.CurTime || immunity.IsImmune)
                    continue;

                RemComp(entity, immunity);
            }

            // Damage
            _dmg.TryChangeDamage(entity, field.Comp.Damage, damageable: dmg, targetPart: TargetBodyPart.Torso);
            // Sound
            if (field.Comp.Sound != null)
                _aud.PlayEntity(field.Comp.Sound, entity, entity, AudioParams.Default.WithVolume(-3f));
            // Immunity frames
            EnsureComp<DamageSquareImmunityComponent>(entity).HasImmunityUntil = _timing.CurTime + TimeSpan.FromSeconds(ImmunityFrames);
        }

        RemComp(field, field.Comp);
    }
}
