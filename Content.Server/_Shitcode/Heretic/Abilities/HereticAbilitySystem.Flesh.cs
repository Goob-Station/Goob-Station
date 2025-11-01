// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Standing;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Ghost.Roles.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Cloning;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.Speech.Muting;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    private static readonly ProtoId<CloningSettingsPrototype> Settings = "FleshMimic";
    private static readonly ProtoId<HTNCompoundPrototype> Compound = "FleshMimicCompound";
    private static readonly SoundSpecifier MimicSpawnSound = new SoundCollectionSpecifier("gib");

    protected override void SubscribeFlesh()
    {
        base.SubscribeFlesh();

        SubscribeLocalEvent<FleshPassiveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<FleshPassiveComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FleshPassiveComponent, GetBodyOrganOverrideEvent<StomachComponent>>(OnGetStomach);
        SubscribeLocalEvent<FleshPassiveComponent, ConsumingFoodEvent>(OnConsumingFood);
        SubscribeLocalEvent<FleshPassiveComponent, ExcludeMetabolismGroupsEvent>(OnExclude);
    }

    private void OnExclude(Entity<FleshPassiveComponent> ent, ref ExcludeMetabolismGroupsEvent args)
    {
        var stomach = ResolveStomach(ent);
        ent.Comp.FleshStomach = stomach;
        if (args.Metabolizer == stomach.Owner)
            return;

        args.Groups ??= [];
        args.Groups.Add("Food");
        args.Groups.Add("Drink");
    }

    private void OnConsumingFood(Entity<FleshPassiveComponent> ent, ref ConsumingFoodEvent args)
    {
        if (args.Volume <= FixedPoint2.Zero)
            return;

        if (!TryComp(ent, out HereticComponent? heretic) || heretic.PathStage <= 0)
            return;

        var multiplier = GetMultiplier((ent.Owner, ent.Comp, heretic), ref args, out var stage, out var multipliersApplied);
        if (!multipliersApplied)
            return;

        var time = TimeSpan.FromSeconds(30) * stage;
        if (heretic.Ascended)
            time += TimeSpan.FromMinutes(1);

        ApplyMultiplier(ent, multiplier * ent.Comp.BaseHealingPerFlesh, time, MartialArtModifierType.Healing);
        ApplyMultiplier(ent, multiplier * ent.Comp.BaseAttackRatePerFlesh, time, MartialArtModifierType.AttackRate);
        ApplyMultiplier(ent, multiplier * ent.Comp.BaseMoveSpeedPerFlesh, time, MartialArtModifierType.MoveSpeed);
        _modifier.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private float GetMultiplier(Entity<FleshPassiveComponent, HereticComponent> ent,
        ref ConsumingFoodEvent args,
        out float stage,
        out bool multipliersApplied)
    {
        stage = (float) Math.Pow((float) ent.Comp2.PathStage, 0.3f);
        var multiplier = args.Volume.Float() * stage;
        var oldMult = multiplier;

        if (HasComp<MobStateComponent>(args.Food))
            multiplier *= ent.Comp1.MobMultiplier;
        if (HasComp<BrainComponent>(args.Food))
            multiplier *= ent.Comp1.BrainMultiplier;
        if (HasComp<BodyPartComponent>(args.Food))
            multiplier *= ent.Comp1.BodyPartMultiplier;
        if (HasComp<OrganComponent>(args.Food))
            multiplier *= ent.Comp1.OrganMultiplier;
        if (HasComp<HumanOrganComponent>(args.Food))
            multiplier *= ent.Comp1.HumanMultiplier;
        if (_tag.HasTag(args.Food, ent.Comp1.MeatTag))
            multiplier *= ent.Comp1.MeatMultiplier;
        if (ent.Comp2.Ascended)
            multiplier *= ent.Comp1.AscensionMultiplier;

        multipliersApplied = oldMult < multiplier;
        return multiplier;
    }

    // Martial arts cuz yeah
    private void ApplyMultiplier(EntityUid uid, float multiplier, TimeSpan time, MartialArtModifierType type)
    {
        if (Math.Abs(multiplier) < 0.01f || time <= TimeSpan.Zero)
            return;

        var multComp = EnsureComp<MartialArtModifiersComponent>(uid);
        multComp.Data.Add(new MartialArtModifierData
        {
            Type = type,
            Multiplier = multiplier + 1f,
            EndTime = Timing.CurTime + time,
        });

        Dirty(uid, multComp);
    }

    private void OnGetStomach(Entity<FleshPassiveComponent> ent, ref GetBodyOrganOverrideEvent<StomachComponent> args)
    {
        var stomach = ResolveStomach(ent);
        ent.Comp.FleshStomach = stomach;
        args.Organ = stomach;
    }

    private void OnMapInit(Entity<FleshPassiveComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.FleshStomach = ResolveStomach(ent);
    }

    private Entity<StomachComponent, OrganComponent> ResolveStomach(Entity<FleshPassiveComponent> ent)
    {
        if (Exists(ent.Comp.FleshStomach) && TryComp(ent.Comp.FleshStomach.Value, out StomachComponent? stomach) &&
            TryComp(ent.Comp.FleshStomach.Value, out OrganComponent? organ))
            return (ent.Comp.FleshStomach.Value, stomach, organ);

        QueueDel(ent.Comp.FleshStomach);

        var solName = StomachSystem.DefaultSolutionName;

        var fleshStomach = SpawnAttachedTo(null, ent.Owner.ToCoordinates());
        var solutionContainer = EnsureComp<SolutionContainerManagerComponent>(fleshStomach);
        _solution.EnsureSolutionPrototype((fleshStomach, solutionContainer), solName, 1984, null, out _);
        _solution.EnsureAllSolutions((fleshStomach, solutionContainer));
        stomach = EnsureComp<StomachComponent>(fleshStomach);
        stomach.DigestionDelay = TimeSpan.FromSeconds(5);
        organ = EnsureComp<OrganComponent>(fleshStomach);
        organ.IntegrityCap = 1984;
        organ.OrganIntegrity = 1984;
        organ.Body = ent;
        Dirty(fleshStomach, organ);
        var metabolizer = EnsureComp<MetabolizerComponent>(fleshStomach);
        metabolizer.UpdateInterval = TimeSpan.FromMilliseconds(100);
        metabolizer.MaxPoisonsProcessable = 10;
        metabolizer.MetabolismGroups = new() { new() { Id = "Food" }, new() { Id = "Drink" } };
        metabolizer.MetabolizerTypes = new() { "Vox" };
        metabolizer.SolutionOnBody = false;
        metabolizer.RemoveEmpty = true;
        metabolizer.SolutionName = solName;
        return (fleshStomach, stomach, organ);
    }

    private void OnDamageChanged(Entity<FleshPassiveComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        var damage = args.DamageDelta.GetTotal();

        if (damage < 5)
            return;

        if (!TryComp(ent, out HereticComponent? heretic) || !heretic.Ascended)
            return;

        ent.Comp.FleshMimics.RemoveAll(x => !Exists(x));
        if (ent.Comp.MaxMimics > 0 && ent.Comp.FleshMimics.Count > ent.Comp.MaxMimics)
        {
            var toHeal = -damage / ent.Comp.FleshMimics.Count * ent.Comp.MimicHealMultiplier;
            foreach (var mimic in ent.Comp.FleshMimics)
            {
                IHateWoundMed(mimic, AllDamage * toHeal, toHeal, toHeal, toHeal);
            }

            return;
        }

        if (CreateFleshMimic(ent, ent, true, true, 100, args.Origin) is not { } clone)
            return;

        ent.Comp.FleshMimics.Add(clone);
        EnsureComp<MutedComponent>(clone);
        EnsureComp<HereticBladeUserBonusDamageComponent>(clone);
    }

    public EntityUid? CreateFleshMimic(EntityUid uid,
        EntityUid user,
        bool giveBlade,
        bool makeGhostRole,
        FixedPoint2 hp,
        EntityUid? hostile)
    {
        var xform = Transform(uid);
        if (!_cloning.TryCloning(uid, _xform.GetMapCoordinates(xform), Settings, out var clone))
            return null;

        _aud.PlayPvs(MimicSpawnSound, xform.Coordinates);

        EntityUid? weapon = null;
        if (!giveBlade && TryComp(uid, out HandsComponent? hands))
        {
            foreach (var held in _hands.EnumerateHeld((uid, hands)))
            {
                if (HasComp<GunComponent>(held))
                {
                    weapon = held;
                    break;
                }

                if (HasComp<MeleeWeaponComponent>(held) && weapon == null)
                    weapon = held;
            }
        }

        var ghoul = _compFactory.GetComponent<GhoulComponent>();
        ghoul.GiveBlade = giveBlade;
        ghoul.TotalHealth = hp;
        ghoul.BoundHeretic = user;
        ghoul.DropOrgansOnDeath = false;
        ghoul.GhostRoleName = "ghostrole-flesh-mimic-name";
        ghoul.GhostRoleDesc = "ghostrole-flesh-mimic-desc";
        if (weapon != null && _cloning.CopyItem(weapon.Value, xform.Coordinates, copyStorage: false) is { } weaponClone)
        {
            if (!_hands.TryPickup(clone.Value, weaponClone, null, false, false, false))
                QueueDel(weaponClone);
            else
            {
                EnsureComp<GhoulWeaponComponent>(weaponClone);
                ghoul.BoundWeapon = weaponClone;
                var cartridgeQuery = GetEntityQuery<CartridgeAmmoComponent>();
                if (TryComp(weaponClone, out ContainerManagerComponent? containerManager))
                {
                    foreach (var container in containerManager.Containers.Values)
                    {
                        foreach (var contained in container.ContainedEntities)
                        {
                            if (!cartridgeQuery.HasComp(contained))
                                EnsureComp<UnremoveableComponent>(contained);
                        }
                    }
                }
            }
        }

        AddComp(clone.Value, ghoul);

        if (_statusEffect.TryGetTime(uid, "KnockedDown", out var knockdownStartEnd))
        {
            var time = knockdownStartEnd.Value.Item2 - Timing.CurTime;
            if (time > TimeSpan.Zero)
                _stun.TryKnockdown(clone.Value, time, true, DropHeldItemsBehavior.NoDrop);
        }

        var damage = EnsureComp<DamageOverTimeComponent>(clone.Value);
        damage.Damage = new DamageSpecifier()
        {
            DamageDict =
            {
                { "Blunt", 0.3 },
                { "Slash", 0.3 },
                { "Piercing", 0.3 },
            }
        };
        damage.MultiplierIncrease = 0.02f;
        damage.IgnoreResistances = true;
        Dirty(clone.Value, damage);

        if (!makeGhostRole)
            RemCompDeferred<GhostTakeoverAvailableComponent>(clone.Value);

        var htn = EnsureComp<HTNComponent>(clone.Value);
        htn.RootTask = new HTNCompoundTask { Task = Compound };
        _npc.SetBlackboard(clone.Value, NPCBlackboard.FollowTarget, user.ToCoordinates(), htn);

        var exception = EnsureComp<FactionExceptionComponent>(clone.Value);
        _npcFaction.IgnoreEntity((clone.Value, exception), user);
        if (user != uid)
            _npcFaction.AggroEntity((clone.Value, exception), uid);
        if (hostile != null && hostile.Value != user)
            _npcFaction.AggroEntity((clone.Value, exception), hostile.Value);

        _htn.Replan(htn);

        return clone.Value;
    }
}
