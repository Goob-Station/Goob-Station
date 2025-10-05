using Content.Shared._Shitmed.Targeting;
using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools.Systems;
using Content.Shared.Body.Systems;
using Robust.Shared.Serialization;
using System.Linq;
using Content.Shared.Medical.Healing;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.Repairable;

public sealed partial class RepairableSystem : EntitySystem
{
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;          // Goob edit
    [Dependency] private readonly HealingSystem _healingSystem = default!;          // Goob edit
    [Dependency] private readonly WoundSystem _wounds = default!;                   // Goob edit

    public override void Initialize()
    {
        SubscribeLocalEvent<RepairableComponent, InteractUsingEvent>(Repair);
        SubscribeLocalEvent<RepairableComponent, RepairFinishedEvent>(OnRepairFinished);
    }

    private void OnRepairFinished(Entity<RepairableComponent> ent,  ref RepairFinishedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp(ent.Owner, out DamageableComponent? damageable) || damageable.TotalDamage == 0)
            return;

        if (TryComp<BodyComponent>(ent.Owner, out var body) && ent.Comp.Damage != null && body != null) // Goob edit : repair entities with bodies
        {
            var targetedWoundable = EntityUid.Invalid;
            if (TryComp<TargetingComponent>(args.User, out var targeting))
            {
                var (partType, symmetry) = _bodySystem.ConvertTargetBodyPart(targeting.Target);
                var targetedBodyPart = _bodySystem.GetBodyChildrenOfType(ent, partType, body, symmetry).ToList().FirstOrDefault();
                targetedWoundable = targetedBodyPart.Id;
            }

            if (!TryComp<DamageableComponent>(targetedWoundable, out var damageableComp))
                return;

            // here we create a fake healing comp
            HealingComponent repairHealing = new HealingComponent();
            repairHealing.Damage = ent.Comp.Damage;
            repairHealing.BloodlossModifier = -100;

            if (!_healingSystem.IsBodyDamaged((ent.Owner, body), null, repairHealing, targetedWoundable))                          // Check if there is anything to heal on the initial limb target
                if (_healingSystem.TryGetNextDamagedPart(ent, repairHealing, out var limbTemp) && limbTemp is not null)      // If not then get the next limb to heal
                    targetedWoundable = limbTemp.Value;

            // Welding removes all bleeding instantly. IPC don't even have blood as i'm writing this so makes 0 sense for them to have bleeds.
            if (TryComp<WoundableComponent>(targetedWoundable, out var woundableComp))
            {
                bool healedBleedWound = false;
                FixedPoint2 modifiedBleedStopAbility = 0;
                healedBleedWound = _wounds.TryHealBleedingWounds(targetedWoundable, repairHealing.BloodlossModifier, out modifiedBleedStopAbility, woundableComp);
                if (healedBleedWound)
                    _popup.PopupPredicted(modifiedBleedStopAbility > 0
                            ? Loc.GetString("rebell-medical-item-stop-bleeding-fully")
                            : Loc.GetString("rebell-medical-item-stop-bleeding-partially"),
                        ent,
                        args.User);
            }

            var damageChanged = _damageableSystem.TryChangeDamage(targetedWoundable, ent.Comp.Damage, true, false, origin: args.User);
            _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(ent.Owner):target} by {damageChanged?.GetTotal()}");

            if (_healingSystem.TryGetNextDamagedPart(ent.Owner, repairHealing, out var _) && args.Used != null) // args.Repeat doesn't work because this current event is a wrapped event of ToolDoAfterEvent
            {
                float delay = ent.Comp.DoAfterDelay;
                if (args.User == args.Target)
                {
                    if (!ent.Comp.AllowSelfRepair)
                        return;

                    delay *= ent.Comp.SelfRepairPenalty;
                }
                args.Handled = _toolSystem.UseTool(args.Used.Value, args.User, ent.Owner, delay, ent.Comp.QualityNeeded, new RepairFinishedEvent(), ent.Comp.FuelCost);
            }
        }
        else if (ent.Comp.Damage != null) // goob edit end
        {
            var damageChanged = _damageableSystem.TryChangeDamage(ent.Owner, ent.Comp.Damage, true, false, origin: args.User);
            _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(ent.Owner):target} by {damageChanged?.GetTotal()}");
        }
        else
        {
            // Repair all damage
            _damageableSystem.SetAllDamage(ent.Owner, damageable, 0);
            _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(ent.Owner):target} back to full health");
        }

        var str = Loc.GetString("comp-repairable-repair", ("target", ent.Owner), ("tool", args.Used!));
        _popup.PopupClient(str, ent.Owner, args.User);

        var ev = new RepairedEvent(ent, args.User);
        RaiseLocalEvent(ent.Owner, ref ev);
    }

    private void Repair(Entity<RepairableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Only try repair the target if it is damaged
        if (!TryComp<DamageableComponent>(ent.Owner, out var damageable) || damageable.TotalDamage == 0)
            return;

        float delay = ent.Comp.DoAfterDelay;

        // Add a penalty to how long it takes if the user is repairing itself
        if (args.User == args.Target)
        {
            if (!ent.Comp.AllowSelfRepair)
                return;

            delay *= ent.Comp.SelfRepairPenalty;
        }

        // Run the repairing doafter
        args.Handled = _toolSystem.UseTool(args.Used, args.User, ent.Owner, delay, ent.Comp.QualityNeeded, new RepairFinishedEvent(), ent.Comp.FuelCost);
    }
}

/// <summary>
/// Event raised on an entity when its successfully repaired.
/// </summary>
/// <param name="Ent"></param>
/// <param name="User"></param>
[ByRefEvent]
public readonly record struct RepairedEvent(Entity<RepairableComponent> Ent, EntityUid User);

[Serializable, NetSerializable]
public sealed partial class RepairFinishedEvent : SimpleDoAfterEvent;
