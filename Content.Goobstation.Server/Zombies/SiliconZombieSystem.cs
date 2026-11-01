using Content.Goobstation.Common.Zombies;
using Content.Shared._Afterlight.Silicons.Synths.Battery;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Emp;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Radio.Components;
using Content.Shared.Zombies;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Zombies;

public sealed partial class SiliconZombieSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private static readonly ProtoId<DamageTypePrototype> PoisonType = "Poison";
    private static readonly ProtoId<DamageTypePrototype> IonType = "Ion";
    private static readonly LocId CorruptingPopup = "silicon-zombie-corrupting";

    [SubscribeLocalEvent]
    private void OnSymptom(Entity<SiliconZombieComponent> ent, ref PendingZombieSymptomEvent args)
    {
        Spark(ent);
        args.Popup = CorruptingPopup;
        args.Cough = false;
    }

    [SubscribeLocalEvent]
    private void OnPendingStartup(Entity<PendingZombieComponent> ent, ref ComponentStartup args)
    {
        if (!HasComp<SiliconZombieComponent>(ent)
            || !TryComp<DamageableComponent>(ent, out var damageable)
            || damageable.Damage.DamageDict.ContainsKey(PoisonType)
            || !damageable.Damage.DamageDict.ContainsKey(IonType))
            return;

        ent.Comp.Damage = new DamageSpecifier(_proto.Index(IonType), ent.Comp.Damage.GetTotal());
    }

    [SubscribeLocalEvent]
    private void OnZombified(Entity<SiliconZombieComponent> ent, ref EntityZombifiedEvent args)
    {
        if (TryComp<SiliconComponent>(ent, out var silicon))
            silicon.BatteryPowered = false;

        RemComp<SynthBatteryComponent>(ent);
        RemComp<IntrinsicRadioTransmitterComponent>(ent);
        RemComp<IntrinsicRadioReceiverComponent>(ent);
        RemComp<ActiveRadioComponent>(ent);
        TintMarkings(ent);
        _popup.PopupEntity(Loc.GetString("silicon-zombie-zombified", ("target", ent.Owner)), ent, PopupType.LargeCaution);
    }

    private void TintMarkings(EntityUid uid)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid)
            || !TryComp<ZombieComponent>(uid, out var zombie))
            return;

        foreach (var (category, markings) in humanoid.MarkingSet.Markings)
        {
            if (category is MarkingCategories.Hair or MarkingCategories.FacialHair)
                continue;

            foreach (var marking in markings)
                marking.SetColor(zombie.SkinColor);
        }

        Dirty(uid, humanoid);
    }

    [SubscribeLocalEvent]
    private void OnEmp(Entity<SiliconZombieComponent> ent, ref EmpPulseEvent args)
    {
        if (CanPurge(ent))
        {
            args.Affected = true;
            Purge(ent);
            return;
        }

        if (!CanReboot(ent))
            return;

        args.Affected = true;
        Reboot(ent);
    }

    private bool CanPurge(EntityUid uid)
    {
        return HasComp<PendingZombieComponent>(uid) && !HasComp<IncurableZombieComponent>(uid);
    }

    private bool CanReboot(EntityUid uid)
    {
        return HasComp<ZombieComponent>(uid)
               && !HasComp<IncurableZombieComponent>(uid)
               && !_mobState.IsAlive(uid);
    }

    private void Purge(Entity<SiliconZombieComponent> ent)
    {
        RemComp<PendingZombieComponent>(ent);
        RemComp<ZombifyOnDeathComponent>(ent);
        Spark(ent);
        _popup.PopupEntity(Loc.GetString("silicon-zombie-purged", ("target", ent.Owner)), ent, PopupType.Medium);
    }

    private void Reboot(Entity<SiliconZombieComponent> ent)
    {
        Spark(ent);

        var ev = new EntityUnZombifiedEvent(false);
        RaiseLocalEvent(ent, ref ev);
    }

    private void Spark(Entity<SiliconZombieComponent> ent)
    {
        SpawnAtPosition(ent.Comp.SparkEffect, Transform(ent).Coordinates);
        _audio.PlayPvs(ent.Comp.SparkSound, ent);
    }
}
