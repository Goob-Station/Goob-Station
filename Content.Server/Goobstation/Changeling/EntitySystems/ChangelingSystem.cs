using Content.Server.DoAfter;
using Content.Server.Forensics;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Server.Zombies;
using Content.Shared.Alert;
using Content.Shared.Changeling.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Store.Components;
using Robust.Server.Audio;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Server.Body.Systems;
using Content.Shared.Polymorph;
using Robust.Shared.Serialization.Manager;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Cuffs;
using Content.Shared.Fluids;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Player;
using Content.Shared.Camera;
using Robust.Shared.Timing;
using Content.Shared.Damage.Components;
using Content.Server.Gravity;
using Content.Server.Stunnable;

namespace Content.Server.Changeling.EntitySystems;

public sealed partial class ChangelingSystem : EntitySystem
{
    // this is one hell of a star wars intro text
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;

    [Dependency] private readonly BlindableSystem _blindable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;

    [Dependency] private readonly PullingSystem _pull = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffs = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ChangelingComponent, MobStateChangedEvent>(OnMobStateChange);

        SubscribeAbilitiesBase();
        SubscribeCustomBehavior();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var comp in EntityManager.EntityQuery<ChangelingComponent>())
        {
            var uid = comp.Owner;

            if (_timing.CurTime < comp.RegenTime)
                continue;

            comp.RegenTime = _timing.CurTime + TimeSpan.FromSeconds(comp.RegenCooldown);

            Cycle(uid, comp);
        }
    }
    public void Cycle(EntityUid uid, ChangelingComponent comp)
    {
        UpdateChemicals(uid, comp);

        if (comp.StrainedMusclesActive)
        {
            var stamina = EnsureComp<StaminaComponent>(uid);
            _stamina.TakeStaminaDamage(uid, 7.5f, visual: false);
            if (stamina.StaminaDamage >= stamina.CritThreshold || _gravity.IsWeightless(uid))
                ToggleStrainedMuscles(uid, comp);
        }
    }

    private void UpdateChemicals(EntityUid uid, ChangelingComponent comp, float? amount = null)
    {
        var chemicals = comp.Chemicals;

        chemicals += amount ?? 1 /*regen*/;

        comp.Chemicals = Math.Clamp(chemicals, 0, comp.MaxChemicals);

        Dirty(uid, comp);

        _alerts.ShowAlert(uid, "Chemicals");
    }

    public void AddDNA(EntityUid uid, ChangelingComponent comp, TransformData data, bool countObjective = false)
    {
        if (comp.AbsorbedDNA.Count >= comp.MaxAbsorbedDNA)
        {
            _popup.PopupEntity(Loc.GetString("changeling-sting-extract-max"), uid, uid);
            return;
        }
        comp.AbsorbedDNA.Add(data);

        if (countObjective)
        {
            if (_mind.TryGetMind(uid, out var mindId, out var mind))
                if (_mind.TryGetObjectiveComp<StealDNAConditionComponent>(mindId, out var objective, mind))
                    objective.DNAStolen += 1;
        }
    }
    public bool TryStealDNA(EntityUid uid, EntityUid target, ChangelingComponent comp, bool countObjective = false)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var appearance)
        || !TryComp<MetaDataComponent>(target, out var metadata)
        || !TryComp<DnaComponent>(target, out var dna)
        || !TryComp<FingerprintComponent>(target, out var fingerprint))
            return false;

        foreach (var storedDNA in comp.AbsorbedDNA)
        {
            if (storedDNA.DNA != null && storedDNA.DNA == dna.DNA)
                return false;
        }

        var data = new TransformData
        {
            Name = metadata.EntityName,
            DNA = dna.DNA,
            Appearance = appearance
        };

        if (fingerprint.Fingerprint != null)
            data.Fingerprint = fingerprint.Fingerprint;

        AddDNA(uid, comp, data, countObjective);

        return true;
    }

    private ChangelingComponent? CopyChangelingComponent(EntityUid target, ChangelingComponent comp)
    {
        var newComp = EnsureComp<ChangelingComponent>(target);
        newComp.AbsorbedDNA = comp.AbsorbedDNA;
        newComp.AbsorbedDNAIndex = comp.AbsorbedDNAIndex;
        newComp.Chemicals = comp.Chemicals;

        newComp.IsInLesserForm = comp.IsInLesserForm;
        newComp.CurrentForm = comp.CurrentForm;

        newComp.TotalAbsorbedEntities = comp.TotalAbsorbedEntities;
        newComp.TotalStolenDNA = comp.TotalStolenDNA;

        return comp;
    }
    public bool TryTransform(EntityUid target, ChangelingComponent comp, bool sting = false, bool persistentDna = false)
    {
        var data = comp.SelectedForm;

        if (data == null)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-fail-self"), target, target);
            return false;
        }
        if (data == comp.CurrentForm)
        {
            _popup.PopupEntity(Loc.GetString("changeling-transform-fail-choose"), target, target);
            return false;
        }

        var locName = Identity.Entity(target, EntityManager);
        EntityUid? newUid = null;
        if (sting)
            newUid = TransformEntity(target, data: data, persistentDna: persistentDna);
        else newUid = TransformEntity(target, data: data, comp: comp, persistentDna: persistentDna);

        if (newUid != null)
        {
            PlayMeatySound((EntityUid) newUid, comp);
            var loc = Loc.GetString("changeling-transform-others", ("user", locName));
            _popup.PopupEntity(loc, (EntityUid) newUid, PopupType.LargeCaution);
        }

        return true;
    }
    private EntityUid? TransformEntity(EntityUid uid, TransformData? data = null, EntProtoId? protoId = null, ChangelingComponent? comp = null, bool persistentDna = false)
    {
        EntProtoId? pid = null;

        if (data != null)
        {
            if (!_proto.TryIndex(data.Appearance.Species, out var species))
                return null;
            pid = species.Prototype;
        }
        else if (protoId != null)
            pid = protoId;
        else return null;

        var config = new PolymorphConfiguration()
        {
            Entity = (EntProtoId) pid,
            TransferDamage = true,
            Forced = true,
            Inventory = PolymorphInventoryChange.Transfer,
            RevertOnCrit = false,
            RevertOnDeath = false
        };
        var newUid = _polymorph.PolymorphEntity(uid, config);

        if (newUid == null)
            return null;

        var newEnt = newUid.Value;

        if (data != null)
        {
            Comp<FingerprintComponent>(newEnt).Fingerprint = data.Fingerprint;
            Comp<DnaComponent>(newEnt).DNA = data.DNA;
            _humanoid.CloneAppearance(data.Appearance.Owner, newEnt);
            _metaData.SetEntityName(newEnt, data.Name);
            var message = Loc.GetString("changeling-transform-finish", ("target", data.Name));
            _popup.PopupEntity(message, newEnt, newEnt);
        }

        RemCompDeferred<PolymorphedEntityComponent>(newEnt);

        if (comp != null)
        {
            // copy our stuff
            var newLingComp = CopyChangelingComponent(newEnt, comp);
            if (!persistentDna && data != null)
                newLingComp?.AbsorbedDNA.Remove(data);
            RemCompDeferred<ChangelingComponent>(uid);

            if (TryComp<StoreComponent>(uid, out var storeComp))
            {
                var storeCompCopy = _serialization.CreateCopy(storeComp, notNullableOverride: true);
                RemComp<StoreComponent>(newUid.Value);
                EntityManager.AddComponent(newUid.Value, storeCompCopy);
            }
        }

        // exceptional comps check
        // there's no foreach for types i believe so i gotta thug it out yandev style.
        if (HasComp<HeadRevolutionaryComponent>(uid))
            EnsureComp<HeadRevolutionaryComponent>(newEnt);
        if (HasComp<RevolutionaryComponent>(uid))
            EnsureComp<RevolutionaryComponent>(newEnt);

        QueueDel(uid);

        return newUid;
    }
    
    public void RemoveAllChangelingEquipment(EntityUid target, ChangelingComponent comp)
    {
        if (comp.Equipment.Values.Count == 0)
            return;

        foreach (var equip in comp.Equipment.Values)
        {
            if (equip == null || equip.Entity == null)
                continue;

            QueueDel(equip.Entity);
            if (equip.Prototype != null)
                comp.Equipment.Remove((EntProtoId) equip.Prototype);
        }

        PlayMeatySound(target, comp);
    }

    private void OnStartup(Entity<ChangelingComponent> ent, ref ComponentStartup args)
    {
        RemComp<HungerComponent>(ent);
        RemComp<ThirstComponent>(ent);
        EnsureComp<ZombieImmuneComponent>(ent);

        // add actions
        foreach (var actionId in ent.Comp.BaseChangelingActions)
            _actions.AddAction(ent, actionId);

        // check for equipment
        if (ent.Comp.Equipment.Count > 0)
        {
            foreach (var equip in ent.Comp.Equipment.Values)
            {
                if (equip != null)
                    TryToggleItem(ent, ent.Comp, equip);
            }
        }
    }

    /// <summary>
    ///     Removes all equipment in case of death. This is done to prevent equipment from dropping on getting gibbed.
    /// </summary>
    private void OnMobStateChange(Entity<ChangelingComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemoveAllChangelingEquipment(ent, ent.Comp);
    }
}
