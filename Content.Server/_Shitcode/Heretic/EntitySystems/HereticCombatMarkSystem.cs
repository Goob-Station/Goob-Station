// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Heretic;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using Content.Shared.Humanoid;
using Content.Server.Body.Components;
using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server.Medical;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticCombatMarkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly ProtectiveBladeSystem _pbs = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public void ApplyMarkEffect(EntityUid target, HereticCombatMarkComponent mark, string? path, EntityUid user)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        switch (path)
        {
            case "Ash":
                _stamina.TakeStaminaDamage(target, 6f * mark.Repetitions);

                var dmg = new DamageSpecifier
                {
                    DamageDict =
                    {
                        { "Heat", 3f * mark.Repetitions },
                    },
                };

                _damageable.TryChangeDamage(target, dmg, origin: user, targetPart: TargetBodyPart.All);
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
                return;
        }

        var repetitions = mark.Repetitions - 1;
        RemComp(target, mark);
        if (repetitions <= 0)
            return;

        // transfers the mark to the next nearby person
        var look = _lookup.GetEntitiesInRange(target, 5f, flags: LookupFlags.Dynamic)
            .Where(x => x != target && HasComp<HumanoidAppearanceComponent>(x) && !HasComp<HereticComponent>(x) && !HasComp<GhoulComponent>(x))
            .ToList();
        if (look.Count == 0)
            return;

        _random.Shuffle(look);
        var lookent = look.First();
        if (!HasComp<HumanoidAppearanceComponent>(lookent) || HasComp<HereticComponent>(lookent))
            return;

        var markComp = EnsureComp<HereticCombatMarkComponent>(lookent);
        markComp.Path = path;
        markComp.Repetitions = repetitions;
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
