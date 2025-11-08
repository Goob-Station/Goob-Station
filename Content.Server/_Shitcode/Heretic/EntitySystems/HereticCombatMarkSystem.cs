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
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Heretic;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using Content.Shared.Humanoid;
using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;
using Content.Server.Heretic.Abilities;
using Content.Server.Medical;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Stunnable;

namespace Content.Server.Heretic.EntitySystems;

public sealed class HereticCombatMarkSystem : SharedHereticCombatMarkSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly ProtectiveBladeSystem _pbs = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StarMarkSystem _starMark = default!;
    [Dependency] private readonly HereticAbilitySystem _ability = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentStartup>(OnStart);
        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentRemove>(OnRemove);

        SubscribeLocalEvent<HereticCosmicMarkComponent, ComponentRemove>(OnCosmicRemove);
    }

    public override bool ApplyMarkEffect(EntityUid target,
        HereticCombatMarkComponent mark,
        string? path,
        EntityUid user,
        HereticComponent heretic)
    {
        if (!base.ApplyMarkEffect(target, mark, path, user, heretic))
            return false;

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
                if (!_mobState.IsDead(target) && !HasComp<GhoulComponent>(target))
                {
                    if (_ability.CreateFleshMimic(target, user, false, true, 50, null) is { } mimic)
                        EnsureComp<FleshMimickedComponent>(target).FleshMimics.Add(mimic);
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
                _stun.KnockdownOrStun(target, TimeSpan.FromSeconds(20), true);
                break;

            case "Void":
                _voidcurse.DoCurse(target, 3);
                break;

            case "Cosmos":
                if (!TryComp(target, out HereticCosmicMarkComponent? cosmicMark))
                    break;

                var targetCoords = Transform(target).Coordinates;
                _starMark.SpawnCosmicField(targetCoords, heretic.PathStage);

                if (Exists(cosmicMark.CosmicDiamondUid))
                {
                    Spawn(cosmicMark.CosmicCloud, targetCoords);
                    var newCoords = Transform(cosmicMark.CosmicDiamondUid.Value).Coordinates;
                    _pulling.StopAllPulls(target);
                    _transform.SetCoordinates(target, newCoords);
                    Spawn(cosmicMark.CosmicCloud, newCoords);
                    Del(cosmicMark.CosmicDiamondUid.Value); // Just in case
                }

                _stun.TryParalyze(target, cosmicMark.ParalyzeTime, true);
                break;

            default:
                return false;
        }

        var repetitions = mark.Repetitions - 1;
        if (repetitions <= 0)
            return true;

        // transfers the mark to the next nearby person
        var look = _lookup.GetEntitiesInRange(target, 5f, flags: LookupFlags.Dynamic)
            .Where(x => x != target && HasComp<HumanoidAppearanceComponent>(x) && !HasComp<HereticComponent>(x) && !HasComp<GhoulComponent>(x))
            .ToList();
        if (look.Count == 0)
            return true;

        _random.Shuffle(look);
        var lookent = look.First();
        if (!HasComp<HumanoidAppearanceComponent>(lookent) || HasComp<HereticComponent>(lookent))
            return true;

        var markComp = EnsureComp<HereticCombatMarkComponent>(lookent);
        markComp.DisappearTime = markComp.MaxDisappearTime;
        markComp.Path = path;
        markComp.Repetitions = repetitions;
        Dirty(lookent, markComp);
        return true;
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

    private void OnRemove(Entity<HereticCombatMarkComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent.Owner))
            return;

        RemComp<HereticCosmicMarkComponent>(ent.Owner);
    }

    private void OnCosmicRemove(Entity<HereticCosmicMarkComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent.Comp.CosmicDiamondUid))
            return;

        Del(ent.Comp.CosmicDiamondUid);
    }
}
