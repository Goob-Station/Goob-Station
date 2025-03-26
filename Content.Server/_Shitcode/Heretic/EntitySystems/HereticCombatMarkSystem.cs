using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using Content.Shared.Humanoid;
using Content.Server.Body.Components;
using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server.Medical;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticCombatMarkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ProtectiveBladeSystem _pbs = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;

    public bool ApplyMarkEffect(EntityUid target, string? path, EntityUid user)
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
                _pbs.AddProtectiveBlade(user);
                break;

            case "Flesh":
                if (TryComp<BloodstreamComponent>(target, out var blood))
                {
                    _blood.TryModifyBleedAmount(target, 5f, blood);
                    _blood.SpillAllSolutions(target, blood);
                }
                break;

            case "Lock":
                // bolts nearby doors
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
                _vomit.Vomit(target);
                break;

            case "Void":
                // set target's temperature to -40C
                // is really OP with the new temperature slowing thing :godo:
                _voidcurse.DoCurse(target);
                break;

            default:
                return false;
        }

        // transfers the mark to the next nearby person
        var look = _lookup.GetEntitiesInRange(target, 5f);
        if (look.Count != 0)
        {
            var lookent = look.ToArray()[0];
            if (HasComp<HumanoidAppearanceComponent>(lookent)
            && !HasComp<HereticComponent>(lookent))
            {
                var markComp = EnsureComp<HereticCombatMarkComponent>(lookent);
                markComp.Path = path;
            }
        }

        return true;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentStartup>(OnStart);
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
}
