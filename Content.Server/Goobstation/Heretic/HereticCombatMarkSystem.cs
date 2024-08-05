using Content.Server.Atmos.EntitySystems;
using Content.Server.Temperature.Systems;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Heretic;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server.Heretic;

public sealed partial class HereticCombatMarkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    public bool ApplyMarkEffect(EntityUid target, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        switch (path)
        {
            case "Ash":
                // gives fire stacks
                _flammable.AdjustFireStacks(target, 5, ignite: true);
                break;

            case "Blade":
                // TODO: add rotating protective blade type shit
                break;

            case "Flesh":
                break;

            case "Lock":
                // bolts all doors
                var lookup = _lookup.GetEntitiesInRange(target, 5f);
                foreach (var door in lookup)
                {
                    if (!TryComp<DoorBoltComponent>(door, out var doorComp))
                        continue;
                    _door.SetBoltsDown((door, doorComp), true);
                }
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/knock.ogg"), target);
                break;

            case "Rust":
                // TODO: add item damage, for now just break random items
                break;

            case "Void":
                // set target's temperature to -20C
                _temperature.ForceChangeTemperature(target, Atmospherics.T0C - 20f);
                break;

            default:
                return false;
        }

        return true;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentStartup>(OnStart);
        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChange);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var comp in EntityQuery<HereticCombatMarkComponent>())
        {
            if (_timing.CurTime > comp.Timer)
                RemComp(comp.Owner, comp);
        }
    }

    private void OnStart(Entity<HereticCombatMarkComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.Timer == TimeSpan.Zero)
            ent.Comp.Timer = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.DisappearTime);
    }
    private void OnDamageChange(Entity<DamageableComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta is null || !args.DamageIncreased)
            return;

        if (!args.Origin.HasValue)
            return;

        if (!TryComp<HereticComponent>(args.Origin.Value, out var hereticComp))
            return;

        if (!TryComp<HereticCombatMarkComponent>(ent, out var mark))
            return;


        if (ApplyMarkEffect(ent, hereticComp.CurrentPath))
            RemComp(ent, mark);
    }
}
