using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Access.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Clumsy;
using Content.Shared.Cluwne;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Ghost;
using Content.Shared.Gibbing.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Jittering;
using Content.Shared.Magic;
using Content.Shared.Mobs.Systems;
using Content.Shared.PDA;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Wizard;

public abstract class SharedSpellsSystem : EntitySystem
{
    #region Dependencies

    [Dependency] protected readonly IMapManager MapManager = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly EntityLookupSystem Lookup = default!;
    [Dependency] protected readonly SharedMapSystem Map = default!;
    [Dependency] protected readonly SharedStunSystem Stun = default!;
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;
    [Dependency] private   readonly INetManager _net = default!;
    [Dependency] private   readonly IGameTiming _timing = default!;
    [Dependency] private   readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private   readonly InventorySystem _inventory = default!;
    [Dependency] private   readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private   readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private   readonly SharedMagicSystem _magic = default!;
    [Dependency] private   readonly SharedPopupSystem _popup = default!;
    [Dependency] private   readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private   readonly SharedBodySystem _body = default!;
    [Dependency] private   readonly DamageableSystem _damageable = default!;
    [Dependency] private   readonly MobStateSystem _mobState = default!;

    #endregion

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CluwneCurseEvent>(OnCluwneCurse);
        SubscribeLocalEvent<BananaTouchEvent>(OnBananaTouch);
        SubscribeLocalEvent<MimeMalaiseEvent>(OnMimeMalaise);
        SubscribeLocalEvent<MagicMissileEvent>(OnMagicMissile);
        SubscribeLocalEvent<DisableTechEvent>(OnDisableTech);
        SubscribeLocalEvent<SmokeSpellEvent>(OnSmoke);
        SubscribeLocalEvent<RepulseEvent>(OnRepulse);
        SubscribeLocalEvent<StopTimeEvent>(OnStopTime);
        SubscribeLocalEvent<CorpseExplosionEvent>(OnCorpseExplosion);
        SubscribeLocalEvent<BlindSpellEvent>(OnBlind);
    }

    #region Spells

    private void OnCluwneCurse(CluwneCurseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        Stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);
        _jitter.DoJitter(ev.Target, ev.JitterStutterDuration, true, status: status);
        _stutter.DoStutter(ev.Target, ev.JitterStutterDuration, true, status);

        EnsureComp<CluwneComponent>(ev.Target);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnBananaTouch(BananaTouchEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        Stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);
        _jitter.DoJitter(ev.Target, ev.JitterStutterDuration, true, status: status);
        _stutter.DoStutter(ev.Target, ev.JitterStutterDuration, true, status);

        var targetWizard = HasComp<WizardComponent>(ev.Target);

        if (!targetWizard)
            EnsureComp<ClumsyComponent>(ev.Target);

        SetGear(ev.Target, ev.Gear, !targetWizard);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnMimeMalaise(MimeMalaiseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        Stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);

        var targetWizard = HasComp<WizardComponent>(ev.Target);

        SetGear(ev.Target, ev.Gear, !targetWizard);

        if (!targetWizard)
            MakeMime(ev.Target);
        else
            _statusEffects.TryAddStatusEffect<MutedComponent>(ev.Target, "Muted", ev.WizardMuteDuration, true, status);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnMagicMissile(MagicMissileEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var coords = Transform(ev.Performer).Coordinates;
        var mapCoords = TransformSystem.ToMapCoordinates(coords);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var spawnCoords = MapManager.TryFindGridAt(mapCoords, out var gridUid, out _)
            ? TransformSystem.WithEntityId(coords, gridUid)
            : new(Map.GetMapOrInvalid(mapCoords.MapId), mapCoords.Position);

        var velocity = Physics.GetMapLinearVelocity(spawnCoords);

        var ghostQuery = GetEntityQuery<GhostComponent>();
        var spectralQuery = GetEntityQuery<SpectralComponent>();

        var targets = Lookup.GetEntitiesInRange<StatusEffectsComponent>(coords, ev.Range, LookupFlags.Dynamic);
        var hasTargets = false;

        foreach (var (target, _) in targets)
        {
            if (target == ev.Performer)
                continue;

            if (ghostQuery.HasComp(target) || spectralQuery.HasComp(target))
                continue;

            hasTargets = true;

            if (_net.IsClient)
                break;

            var missile = Spawn(ev.Proto, mapCoords);
            EnsureComp<PreventCollideComponent>(missile).Uid = ev.Performer;
            EnsureComp<HomingProjectileComponent>(missile).Target = target;
            _gunSystem.SetTarget(missile, target);

            var direction = TransformSystem.GetMapCoordinates(target).Position - mapCoords.Position;
            _gunSystem.ShootProjectile(missile, direction, velocity, ev.Performer, ev.Performer, ev.ProjectileSpeed);
        }

        if (!hasTargets)
        {
            _popup.PopupClient(Loc.GetString("spell-fail-no-targets"), ev.Performer, ev.Performer);
            return;
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnDisableTech(DisableTechEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        Emp(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnSmoke(SmokeSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        SpawnSmoke(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnRepulse(RepulseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        Repulse(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnStopTime(StopTimeEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (_net.IsServer)
        {
            var effect = Spawn(ev.Proto, TransformSystem.GetMapCoordinates(ev.Performer));
            EnsureComp<PreventCollideComponent>(effect).Uid = ev.Performer; // Just in case
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnCorpseExplosion(CorpseExplosionEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<BorgChassisComponent>(ev.Target))
        {
            _popup.PopupClient(Loc.GetString("spell-fail-borg"), ev.Performer, ev.Performer);
            return;
        }

        if (!_mobState.IsDead(ev.Target))
        {
            _popup.PopupClient(Loc.GetString("spell-fail-not-dead"), ev.Performer, ev.Performer);
            return;
        }

        var coords = TransformSystem.GetMapCoordinates(ev.Target);

        if (_timing.IsFirstTimePredicted)
            _body.GibBody(ev.Target, contents: GibContentsOption.Gib);

        ExplodeCorpse(ev);

        var targets = Lookup.GetEntitiesInRange<DamageableComponent>(coords, ev.KnockdownRange);
        var ghostQuery = GetEntityQuery<GhostComponent>();
        var spectralQuery = GetEntityQuery<SpectralComponent>();
        var statusQuery = GetEntityQuery<StatusEffectsComponent>();
        var bodyPartQuery = GetEntityQuery<BodyPartComponent>();
        foreach (var (target, damageable) in targets)
        {
            if (target == ev.Performer || target == ev.Target)
                continue;

            if (ghostQuery.HasComp(target) || spectralQuery.HasComp(target) || bodyPartQuery.HasComp(target))
                continue;

            var range = (TransformSystem.GetMapCoordinates(target).Position - coords.Position).Length();

            range = MathF.Max(1f, range);

            _damageable.TryChangeDamage(target,
                ev.Damage / range,
                damageable: damageable,
                origin: ev.Performer,
                targetPart: TargetBodyPart.All);

            if (!statusQuery.TryComp(target, out var status))
                continue;

            if (HasComp<SiliconComponent>(target) || HasComp<BorgChassisComponent>(target))
                Stun.TryParalyze(target, ev.SiliconStunTime / range, true, status);
            else
                Stun.TryKnockdown(target, ev.KnockdownTime / range, true, status);
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnBlind(BlindSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<GhostComponent>(ev.Target) || HasComp<SpectralComponent>(ev.Target))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        _statusEffects.TryAddStatusEffect<TemporaryBlindnessComponent>(ev.Target,
            "TemporaryBlindness",
            ev.BlindDuration,
            true,
            status);

        _statusEffects.TryAddStatusEffect<BlurryVisionComponent>(ev.Target,
            "BlurryVision",
            ev.BlurDuration,
            true,
            status);

        if (_net.IsServer)
        {
            if (TryComp(ev.Target, out VocalComponent? vocal) && !HasComp<BorgChassisComponent>(ev.Target))
                Emote(ev.Target, vocal.ScreamId);

            Spawn(ev.Effect, Transform(ev.Target).Coordinates);
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    #endregion

    #region Helpers

    private void SetGear(EntityUid uid, Dictionary<string, EntProtoId> gear, bool makeUnremoveable = true)
    {
        if (_net.IsClient)
            return;

        if (!TryComp(uid, out InventoryComponent? inventoryComponent))
            return;

        foreach (var (slot, item) in gear)
        {
            _inventory.TryUnequip(uid, slot, true, true, false, inventoryComponent);

            var ent = Spawn(item, Transform(uid).Coordinates);
            if (!_inventory.TryEquip(uid, ent, slot, true, true, false, inventoryComponent))
            {
                Del(ent);
                continue;
            }

            if (slot == "id" &&
                TryComp(ent, out PdaComponent? pdaComponent) &&
                TryComp<IdCardComponent>(pdaComponent.ContainedId, out var id))
                id.FullName = MetaData(uid).EntityName;

            if (makeUnremoveable && HasComp<ClothingComponent>(ent))
                EnsureComp<UnremoveableComponent>(ent);
        }
    }

    #endregion

    #region ServerMethods

    protected virtual void MakeMime(EntityUid uid)
    {
    }

    protected virtual void Emp(DisableTechEvent ev)
    {
    }

    protected virtual void SpawnSmoke(SmokeSpellEvent ev)
    {
    }

    protected virtual void Repulse(RepulseEvent ev)
    {
    }

    protected virtual void ExplodeCorpse(CorpseExplosionEvent ev)
    {
    }

    protected virtual void Emote(EntityUid uid, string emoteId)
    {
    }

    #endregion
}
