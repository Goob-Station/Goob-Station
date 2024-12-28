using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Clumsy;
using Content.Shared.Cluwne;
using Content.Shared.Inventory;
using Content.Shared.Jittering;
using Content.Shared.Magic;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Goobstation.Wizard;

public abstract class SharedSpellsSystem : EntitySystem
{
    #region Dependencies

    [Dependency] protected readonly StatusEffectsSystem StatusEffects = default!;
    [Dependency] protected readonly InventorySystem Inventory = default!;
    [Dependency] private   readonly SharedStunSystem _stun = default!;
    [Dependency] private   readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private   readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private   readonly SharedMagicSystem _magic = default!;
    [Dependency] private   readonly EntityLookupSystem _lookup = default!;
    [Dependency] private   readonly SharedPopupSystem _popup = default!;
    [Dependency] private   readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private   readonly SharedTransformSystem _transform = default!;
    [Dependency] private   readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private   readonly SharedMapSystem _map = default!;
    [Dependency] private   readonly IMapManager _mapManager = default!;
    [Dependency] private   readonly INetManager _net = default!;

    #endregion

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CluwneCurseEvent>(OnCluwneCurse);
        SubscribeLocalEvent<BananaTouchEvent>(OnBananaTouch);
        SubscribeLocalEvent<MimeMalaiseEvent>(OnMimeMalaise);
        SubscribeLocalEvent<MagicMissileEvent>(OnMagicMissile);
    }

    #region Spells

    private void OnCluwneCurse(CluwneCurseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        _stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);
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

        _stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);
        _jitter.DoJitter(ev.Target, ev.JitterStutterDuration, true, status: status);
        _stutter.DoStutter(ev.Target, ev.JitterStutterDuration, true, status);

        var targetWizard = HasComp<WizardComponent>(ev.Target);

        if (!targetWizard)
            EnsureComp<ClumsyComponent>(ev.Target);

        SetGear(ev.Target,
            ev.Gear,
            targetWizard ? SlotFlags.NONE : SlotFlags.MASK | SlotFlags.INNERCLOTHING | SlotFlags.FEET);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnMimeMalaise(MimeMalaiseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        _stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);

        MakeMime(ev, status);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnMagicMissile(MagicMissileEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var coords = Transform(ev.Performer).Coordinates;
        var mapCoords = _transform.ToMapCoordinates(coords);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var spawnCoords = _mapManager.TryFindGridAt(mapCoords, out var gridUid, out _)
            ? _transform.WithEntityId(coords, gridUid)
            : new(_map.GetMapOrInvalid(mapCoords.MapId), mapCoords.Position);

        var velocity = _physics.GetMapLinearVelocity(spawnCoords);

        var targets = _lookup.GetEntitiesInRange<StatusEffectsComponent>(coords, ev.Range, LookupFlags.Dynamic);
        var hasTargets = false;

        foreach (var (target, _) in targets)
        {
            if (target == ev.Performer)
                continue;

            hasTargets = true;

            if (_net.IsClient)
                break;

            var missile = Spawn(ev.Proto, mapCoords);
            EnsureComp<PreventCollideComponent>(missile).Uid = ev.Performer;
            EnsureComp<HomingProjectileComponent>(missile).Target = target;
            _gunSystem.SetTarget(missile, target);

            var direction = _transform.GetMapCoordinates(target).Position - mapCoords.Position;
            _gunSystem.ShootProjectile(missile, direction, velocity, ev.Performer, ev.Performer, ev.ProjectileSpeed);
        }

        if (!hasTargets)
        {
            _popup.PopupClient(Loc.GetString("spell-no-targets"), ev.Performer, ev.Performer);
            return;
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    #endregion

    #region Helpers

    protected virtual void MakeMime(MimeMalaiseEvent ev, StatusEffectsComponent? status = null)
    {
    }

    protected virtual void SetGear(EntityUid uid, string gear, SlotFlags unremoveableClothingFlags = SlotFlags.NONE)
    {
    }

    #endregion
}
