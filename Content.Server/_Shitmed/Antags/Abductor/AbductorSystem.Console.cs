// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared._Shitmed.ItemSwitch;
using Content.Shared._Shitmed.ItemSwitch.Components;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.UserInterface;
using Robust.Shared.Spawners;
using System.Linq;

namespace Content.Server._Shitmed.Antags.Abductor;

public sealed partial class AbductorSystem : SharedAbductorSystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly SharedItemSwitchSystem _itemSwitch = default!;
    public void InitializeConsole()
    {
        SubscribeLocalEvent<AbductorConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUIOpen);
        SubscribeLocalEvent<AbductConditionComponent, ObjectiveGetProgressEvent>(OnAbductGetProgress);

        Subs.BuiEvents<AbductorConsoleComponent>(AbductorConsoleUIKey.Key, subs => subs.Event<AbductorAttractBuiMsg>(OnAttractBuiMsg));
        Subs.BuiEvents<AbductorConsoleComponent>(AbductorConsoleUIKey.Key, subs => subs.Event<AbductorCompleteExperimentBuiMsg>(OnCompleteExperimentBuiMsg));
        Subs.BuiEvents<AbductorConsoleComponent>(AbductorConsoleUIKey.Key, subs => subs.Event<AbductorVestModeChangeBuiMsg>(OnVestModeChangeBuiMsg));
        Subs.BuiEvents<AbductorConsoleComponent>(AbductorConsoleUIKey.Key, subs => subs.Event<AbductorLockBuiMsg>(OnVestLockBuiMsg));
        SubscribeLocalEvent<AbductorComponent, AbductorAttractDoAfterEvent>(OnDoAfterAttract);
    }
    private void OnAbductGetProgress(Entity<AbductConditionComponent> ent, ref ObjectiveGetProgressEvent args)
        => args.Progress = AbductProgress(ent.Comp, _number.GetTarget(ent.Owner));

    private float AbductProgress(AbductConditionComponent comp, int target)
        => target == 0 ? 1f : MathF.Min(comp.Abducted / (float) target, 1f);

    private void OnVestModeChangeBuiMsg(EntityUid uid, AbductorConsoleComponent component, AbductorVestModeChangeBuiMsg args)
    {
        if (component.Armor != null)
            _itemSwitch.Switch(GetEntity(component.Armor.Value), args.Mode.ToString());
    }

    private void OnVestLockBuiMsg(Entity<AbductorConsoleComponent> ent, ref AbductorLockBuiMsg args)
    {
        if (ent.Comp.Armor != null && GetEntity(ent.Comp.Armor.Value) is EntityUid armor)
            if (TryComp<UnremoveableComponent>(armor, out var unremoveable))
                RemComp(armor, unremoveable);
            else
                AddComp<UnremoveableComponent>(armor);
    }

    private void OnCompleteExperimentBuiMsg(EntityUid uid, AbductorConsoleComponent component, AbductorCompleteExperimentBuiMsg args)
    {
        if (component.Experimentator != null
            && GetEntity(component.Experimentator) is EntityUid experimentatorId
            && TryComp<AbductorExperimentatorComponent>(experimentatorId, out var experimentatorComp))
        {
            var container = _container.GetContainer(experimentatorId, experimentatorComp.ContainerId);
            var victim = container.ContainedEntities.FirstOrDefault(HasComp<AbductorVictimComponent>);
            if (victim != default && TryComp(victim, out AbductorVictimComponent? victimComp))
            {
                if (victimComp.Implanted
                    && TryComp<MindContainerComponent>(args.Actor, out var mindContainer)
                    && mindContainer.Mind.HasValue
                    && TryComp<MindComponent>(mindContainer.Mind.Value, out var mind)
                    && mind.Objectives.FirstOrDefault(HasComp<AbductConditionComponent>) is EntityUid objId
                    && TryComp<AbductConditionComponent>(objId, out var condition)
                    && !condition.AbductedHashs.Contains(GetNetEntity(victim)))
                {
                    condition.AbductedHashs.Add(GetNetEntity(victim));
                    condition.Abducted++;
                }
                _audioSystem.PlayPvs("/Audio/Voice/Human/wilhelm_scream.ogg", experimentatorId);

                if (victimComp.Position is not null)
                    _xformSys.SetCoordinates(victim, victimComp.Position.Value);
            }
        }
    }

    private void OnAttractBuiMsg(Entity<AbductorConsoleComponent> ent, ref AbductorAttractBuiMsg args)
    {
        if (ent.Comp.Target == null || ent.Comp.AlienPod == null) return;
        var target = GetEntity(ent.Comp.Target.Value);
        EnsureComp<TransformComponent>(target, out var xform);
        var effectEnt = SpawnAttachedTo(TeleportationEffectEntity, xform.Coordinates);
        _xformSys.SetParent(effectEnt, target);
        EnsureComp<TimedDespawnComponent>(effectEnt, out var despawnEffectEntComp);
        despawnEffectEntComp.Lifetime = 3.0f;
        _audioSystem.PlayPvs("/Audio/_Shitmed/Misc/alien_teleport.ogg", effectEnt);

        var telepad = GetEntity(ent.Comp.AlienPod.Value);
        var telepadXform = EnsureComp<TransformComponent>(telepad);
        var effect = _entityManager.SpawnEntity(TeleportationEffect, telepadXform.Coordinates);
        EnsureComp<TimedDespawnComponent>(effect, out var despawnComp);
        despawnComp.Lifetime = 3.0f;
        _audioSystem.PlayPvs("/Audio/_Shitmed/Misc/alien_teleport.ogg", effect);

        var @event = new AbductorAttractDoAfterEvent(GetNetCoordinates(telepadXform.Coordinates), GetNetEntity(target));
        ent.Comp.Target = null;
        var doAfter = new DoAfterArgs(EntityManager, args.Actor, TimeSpan.FromSeconds(3), @event, args.Actor)
        {
            BreakOnDamage = false,
            BreakOnDropItem = false,
            BreakOnHandChange = false,
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
        };
        _doAfter.TryStartDoAfter(doAfter);
    }
    private void OnDoAfterAttract(Entity<AbductorComponent> ent, ref AbductorAttractDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var victim = GetEntity(args.Victim);
        if (_pullingSystem.IsPulling(victim))
        {
            if (!TryComp<PullerComponent>(victim, out var pullerComp)
                || pullerComp.Pulling == null
                || !TryComp<PullableComponent>(pullerComp.Pulling.Value, out var pullableComp)
                || !_pullingSystem.TryStopPull(pullerComp.Pulling.Value, pullableComp)) return;
        }
        if (_pullingSystem.IsPulled(victim))
        {
            if (!TryComp<PullableComponent>(victim, out var pullableComp)
                || !_pullingSystem.TryStopPull(victim, pullableComp)) return;
        }
        _xformSys.SetCoordinates(victim, GetCoordinates(args.TargetCoordinates));
    }
    private void OnBeforeActivatableUIOpen(Entity<AbductorConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args) => UpdateGui(ent.Comp.Target, ent);

    protected override void UpdateGui(NetEntity? target, Entity<AbductorConsoleComponent> computer)
    {
        string? targetName = null;
        string? victimName = null;
        if (target.HasValue && TryComp(GetEntity(target.Value), out MetaDataComponent? metadata))
            targetName = metadata?.EntityName;

        var armorLock = false;
        var armorMode = AbductorArmorModeType.Stealth;

        if (computer.Comp.Armor != null)
        {
            if (HasComp<UnremoveableComponent>(GetEntity(computer.Comp.Armor.Value)))
                armorLock = true;
            if (TryComp<ItemSwitchComponent>(GetEntity(computer.Comp.Armor.Value), out var switchVest) && Enum.TryParse<AbductorArmorModeType>(switchVest.State, ignoreCase: true, out var State))
                armorMode = State;
        }

        if (computer.Comp.AlienPod == null)
        {
            var xform = EnsureComp<TransformComponent>(computer.Owner);
            var alienpad = _entityLookup.GetEntitiesInRange<AbductorAlienPadComponent>(xform.Coordinates, 4, LookupFlags.Approximate | LookupFlags.Dynamic)
                .FirstOrDefault().Owner;
            if (alienpad != default)
                computer.Comp.AlienPod = GetNetEntity(alienpad);
        }

        if (computer.Comp.Experimentator == null)
        {
            var xform = EnsureComp<TransformComponent>(computer.Owner);
            var experimentator = _entityLookup.GetEntitiesInRange<AbductorExperimentatorComponent>(xform.Coordinates, 4, LookupFlags.Approximate | LookupFlags.Dynamic)
                .FirstOrDefault().Owner;
            if (experimentator != default)
                computer.Comp.Experimentator = GetNetEntity(experimentator);
        }

        if (computer.Comp.Experimentator != null
            && GetEntity(computer.Comp.Experimentator) is EntityUid experimentatorId
            && TryComp<AbductorExperimentatorComponent>(experimentatorId, out var experimentatorComp))
        {
            var container = _container.GetContainer(experimentatorId, experimentatorComp.ContainerId);
            var victim = container.ContainedEntities.FirstOrDefault(e => HasComp<AbductorVictimComponent>(e));
            if (victim != default && TryComp(victim, out MetaDataComponent? victimMetadata))
                victimName = victimMetadata?.EntityName;
        }

        _uiSystem.SetUiState(computer.Owner, AbductorConsoleUIKey.Key, new AbductorConsoleBuiState()
        {
            Target = target,
            TargetName = targetName,
            VictimName = victimName,
            AlienPadFound = computer.Comp.AlienPod != default,
            ExperimentatorFound = computer.Comp.Experimentator != default,
            ArmorFound = computer.Comp.Armor != default,
            ArmorLocked = armorLock,
            CurrentArmorMode = armorMode
        });
    }
}