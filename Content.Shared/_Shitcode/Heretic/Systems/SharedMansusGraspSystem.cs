// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.BackStab;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedMansusGraspSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BackStabSystem _backstab = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedVoidCurseSystem _voidCurse = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public bool TryApplyGraspEffectAndMark(EntityUid user,
        HereticComponent hereticComp,
        EntityUid target,
        EntityUid? grasp)
    {
        if (hereticComp.CurrentPath == null)
            return true;

        if (hereticComp.PathStage >= 2)
        {
            if (!ApplyGraspEffect((user, hereticComp), target, grasp))
                return false;
        }

        if (hereticComp.PathStage >= 4 && HasComp<StatusEffectsComponent>(target))
        {
            var markComp = EnsureComp<HereticCombatMarkComponent>(target);
            markComp.DisappearTime = markComp.MaxDisappearTime;
            markComp.Path = hereticComp.CurrentPath;
            markComp.Repetitions = hereticComp.CurrentPath == "Ash" ? 5 : 1;
            Dirty(target, markComp);
        }

        return true;
    }

    public bool ApplyGraspEffect(Entity<HereticComponent> user, EntityUid target, EntityUid? grasp)
    {
        var (performer, heretic) = user;

        switch (heretic.CurrentPath)
        {
            case "Ash":
            {
                var timeSpan = TimeSpan.FromSeconds(5f);
                _statusEffect.TryAddStatusEffect(target,
                    TemporaryBlindnessSystem.BlindingStatusEffect,
                    timeSpan,
                    false,
                    TemporaryBlindnessSystem.BlindingStatusEffect);
                break;
            }

            case "Blade":
            {
                if (grasp != null && heretic.PathStage >= 7 && _tag.HasTag(target, "HereticBladeBlade"))
                {
                    // empowering blades and shit
                    var infusion = EnsureComp<MansusInfusedComponent>(target);
                    infusion.AvailableCharges = infusion.MaxCharges;
                    break;
                }

                // small stun if the person is looking away or laying down
                if (_backstab.TryBackstab(target, performer, Angle.FromDegrees(45d)))
                {
                    _stun.TryParalyze(target, TimeSpan.FromSeconds(1.5f), true);
                    _damage.TryChangeDamage(target,
                        new DamageSpecifier(_proto.Index<DamageTypePrototype>("Slash"), 10),
                        ignoreResistances: true,
                        origin: performer,
                        targetPart: TargetBodyPart.Torso);
                }

                break;
            }

            case "Lock":
            {
                if (!TryComp<DoorComponent>(target, out var door))
                    break;

                if (TryComp<DoorBoltComponent>(target, out var doorBolt))
                    _door.SetBoltsDown((target, doorBolt), false);

                _door.StartOpening(target, door);
                _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hereticknock.ogg"),
                    target,
                    user);
                break;
            }

            case "Flesh":
            {
                if (TryComp<MobStateComponent>(target, out var mobState) && mobState.CurrentState == MobState.Dead)
                {
                    if (HasComp<GhoulComponent>(target))
                    {
                        if (_net.IsServer)
                            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-target-ghoul"), user, user);
                        break;
                    }

                    if (!_mind.TryGetMind(target, out _, out _))
                    {
                        if (_net.IsServer)
                            _popup.PopupEntity(Loc.GetString("heretic-ability-fail-target-no-mind"), user, user);
                        break;
                    }

                    var ghoul = _compFactory.GetComponent<GhoulComponent>();
                    ghoul.BoundHeretic = GetNetEntity(performer);
                    ghoul.GiveBlade = true;

                    AddComp(target, ghoul);
                }

                break;
            }

            case "Void":
            {
                _voidCurse.DoCurse(target, 2);
                break;
            }

            case "Rust":
            {
                if (TryComp(target, out StationAiHolderComponent? aiHolder)) // Kill AI
                    QueueDel(aiHolder.Slot.ContainerSlot?.ContainedEntity);
                else if (HasComp<RustGraspComponent>(grasp) && _tag.HasAnyTag(target, "Wall", "Catwalk") ||
                         HasComp<HereticRitualRuneComponent>(
                             target)) // If we have rust grasp and targeting a wall (or a catwalk) - do nothing, let other methods handle that. Also don't damage transmutation rune.
                    return false;
                else if (TryComp(target, out DamageableComponent? damageable) && // Is it even damageable?
                         !_tag.HasTag(target, "Meat") && // Is it not organic body part or organ?
                         !HasComp<ShadowCloakEntityComponent>(target) && // No instakilling shadow cloak heretics
                         (!HasComp<MobStateComponent>(target) || HasComp<SiliconComponent>(target) ||
                          HasComp<BorgChassisComponent>(target) ||
                          _tag.HasTag(target, "Bot"))) // Check for ingorganic target
                {
                    _damage.TryChangeDamage(target,
                        new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), 500),
                        ignoreResistances: true,
                        damageable: damageable,
                        origin: performer,
                        targetPart: TargetBodyPart.Torso);
                }

                break;
            }

            default:
                return true;
        }

        return true;
    }
}
