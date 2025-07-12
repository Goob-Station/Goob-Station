using Content.Server.Audio;
using Content.Server.Power.Components;
using Content.Server.Electrocution;
using Content.Server.Lightning;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Ghost;
using Content.Server.Revenant.EntitySystems;
using Content.Shared.Audio;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.GameTicking;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Verbs;
using Content.Shared.StatusEffect;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Construction.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Power;
using Content.Shared.Weapons.Melee.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Psionics.Glimmer;

public sealed class GlimmerReactiveSystem : EntitySystem
{
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _sharedAmbientSoundSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private readonly SharedDestructibleSystem _destructibleSystem = default!;
    [Dependency] private readonly GhostSystem _ghostSystem = default!;
    [Dependency] private readonly RevenantSystem _revenantSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLightSystem = default!;
    private ISawmill _sawmill = default!;

    public float Accumulator = 0;
    public const float UpdateFrequency = 15f;
    public float BeamCooldown = 3;
    public GlimmerTier LastGlimmerTier = GlimmerTier.Minimal;
    public bool GhostsVisible = false;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);

        SubscribeLocalEvent<SharedGlimmerReactiveComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, GlimmerTierChangedEvent>(OnTierChanged);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, GetVerbsEvent<AlternativeVerb>>(AddShockVerb);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, DestructionEventArgs>(OnDestroyed);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
        SubscribeLocalEvent<SharedGlimmerReactiveComponent, AttemptMeleeThrowOnHitEvent>(OnMeleeThrowOnHitAttempt);
    }

    /// <summary>
    /// Update relevant state on an Entity.
    /// </summary>
    /// <param name="glimmerTierDelta">The number of steps in tier
    /// difference since last update. This can be zero for the sake of
    /// toggling the enabled states.</param>
    private void UpdateEntityState(EntityUid uid, SharedGlimmerReactiveComponent component, GlimmerTier currentGlimmerTier, int glimmerTierDelta)
    {
        var isEnabled = true;

        if (component.RequiresApcPower
            && TryComp(uid, out ApcPowerReceiverComponent? apcPower))
            isEnabled = apcPower.Powered;

        _appearanceSystem.SetData(uid, GlimmerReactiveVisuals.GlimmerTier, isEnabled ? currentGlimmerTier : GlimmerTier.Minimal);

        // Update ambient sound
        if (TryComp(uid, out GlimmerSoundComponent? glimmerSound)
            && TryComp(uid, out AmbientSoundComponent? ambientSoundComponent)
            && glimmerSound.GetSound(currentGlimmerTier, out SoundSpecifier? spec)
            && spec != null)
            _sharedAmbientSoundSystem.SetSound(uid, spec, ambientSoundComponent);

        // Update point light
        if (component.ModulatesPointLight
            && _pointLightSystem.TryGetLight(uid, out var pointLight))
        {
            _pointLightSystem.SetEnabled(uid, isEnabled ? currentGlimmerTier != GlimmerTier.Minimal : false, pointLight);
            _pointLightSystem.SetEnergy(uid, pointLight.Energy + glimmerTierDelta * component.GlimmerToLightEnergyFactor, pointLight);
            _pointLightSystem.SetRadius(uid, pointLight.Radius + glimmerTierDelta * component.GlimmerToLightRadiusFactor, pointLight);
        }

    }

    /// <summary>
    /// Track when the component comes online so it can be given the
    /// current status of the glimmer tier, if it wasn't around when an
    /// update went out.
    /// </summary>
    private void OnMapInit(EntityUid uid, SharedGlimmerReactiveComponent component, MapInitEvent args)
    {
        if (component.RequiresApcPower && !HasComp<ApcPowerReceiverComponent>(uid))
            _sawmill.Warning($"{ToPrettyString(uid)} had RequiresApcPower set to true but no ApcPowerReceiverComponent was found on init.");

        UpdateEntityState(uid, component, LastGlimmerTier, (int) LastGlimmerTier);
    }

    /// <summary>
    /// Reset the glimmer tier appearance data if the component's removed,
    /// just in case some objects can temporarily become reactive to the
    /// glimmer.
    /// </summary>
    private void OnComponentRemove(EntityUid uid, SharedGlimmerReactiveComponent component, ComponentRemove args)
    {
        UpdateEntityState(uid, component, GlimmerTier.Minimal, -1 * (int) LastGlimmerTier);
    }

    /// <summary>
    /// If the Entity has RequiresApcPower set to true, this will force an
    /// update to the entity's state.
    /// </summary>
    private void OnPowerChanged(EntityUid uid, SharedGlimmerReactiveComponent component, ref PowerChangedEvent args)
    {
        if (component.RequiresApcPower)
            UpdateEntityState(uid, component, LastGlimmerTier, 0);
    }

    /// <summary>
    ///     Enable / disable special effects from higher tiers.
    /// </summary>
    private void OnTierChanged(EntityUid uid, SharedGlimmerReactiveComponent component, GlimmerTierChangedEvent args)
    {
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var receiver))
            return;

        if (args.CurrentTier >= GlimmerTier.Dangerous)
        {
            if (!Transform(uid).Anchored)
                AnchorOrExplode(uid);

            receiver.PowerDisabled = false;
            receiver.NeedsPower = false;
        }
        else
        {
            receiver.NeedsPower = true;
        }
    }

    private void AddShockVerb(EntityUid uid, SharedGlimmerReactiveComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess
            || !args.CanInteract
            || !TryComp<ApcPowerReceiverComponent>(uid, out var receiver)
            || receiver.NeedsPower)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                _sharedAudioSystem.PlayPvs(component.ShockNoises, args.User);
                _electrocutionSystem.TryDoElectrocution(args.User, null, (int) _glimmerSystem.GlimmerOutput / 200, TimeSpan.FromSeconds(_glimmerSystem.GlimmerOutput / 100), false);
            },
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png")),
            Text = Loc.GetString("power-switch-component-toggle-verb"),
            Priority = -3
        };
        args.Verbs.Add(verb);
    }

    private void OnDamageChanged(EntityUid uid, SharedGlimmerReactiveComponent component, DamageChangedEvent args)
    {
        if (args.Origin == null
            || !_random.Prob((float) _glimmerSystem.GetGlimmerEquilibriumRatio() / 10))
            return;

        var tier = _glimmerSystem.GetGlimmerTier();
        if (tier < GlimmerTier.High)
            return;
        Beam(uid, args.Origin.Value, tier);
    }

    private void OnDestroyed(EntityUid uid, SharedGlimmerReactiveComponent component, DestructionEventArgs args)
    {
        Spawn("MaterialBluespace1", Transform(uid).Coordinates);

        var tier = _glimmerSystem.GetGlimmerTier();
        if (tier < GlimmerTier.High)
            return;

        var glimmer = (float) _glimmerSystem.GlimmerOutput;
        var totalIntensity = glimmer * 2;
        var slope = 11 - glimmer / 100;
        var maxIntensity = 20;

        var removed = glimmer * _random.NextFloat(0.1f, 0.15f);
        _glimmerSystem.DeltaGlimmerInput(-removed);
        BeamRandomNearProber(uid, (int) glimmer / 350, glimmer / 50);
        _explosionSystem.QueueExplosion(uid, "Default", totalIntensity, slope, maxIntensity);
    }

    private void OnUnanchorAttempt(EntityUid uid, SharedGlimmerReactiveComponent component, UnanchorAttemptEvent args)
    {
        if (_glimmerSystem.GetGlimmerTier() < GlimmerTier.Dangerous)
            return;

        _sharedAudioSystem.PlayPvs(component.ShockNoises, args.User);
        _electrocutionSystem.TryDoElectrocution(args.User, null, (int) _glimmerSystem.GlimmerOutput / 200, TimeSpan.FromSeconds(_glimmerSystem.GlimmerOutput / 100), false);
        args.Cancel();
    }

    public void BeamRandomNearProber(EntityUid prober, int targets, float range = 10f)
    {
        List<EntityUid> targetList = new();
        foreach (var (target, status) in _entityLookupSystem.GetEntitiesInRange<StatusEffectsComponent>(_transformSystem.GetMapCoordinates(prober), range))
            if (status.AllowedEffects.Contains("Electrocution"))
                targetList.Add(target);

        foreach (var reactive in _entityLookupSystem.GetEntitiesInRange<SharedGlimmerReactiveComponent>(_transformSystem.GetMapCoordinates(prober), range))
            targetList.Add(reactive);

        _random.Shuffle(targetList);
        foreach (var target in targetList)
        {
            if (targets <= 0)
                return;

            Beam(prober, target, _glimmerSystem.GetGlimmerTier(), false);
            targets--;
        }
    }

    private void Beam(EntityUid prober, EntityUid target, GlimmerTier tier, bool obeyCD = true)
    {
        if (obeyCD && BeamCooldown != 0
            || Deleted(prober)
            || Deleted(target))
            return;

        var lxform = Transform(prober);
        var txform = Transform(target);

        if (!lxform.Coordinates.TryDistance(EntityManager, txform.Coordinates, out var distance))
            return;
        if (distance > _glimmerSystem.GlimmerOutput / 100)
            return;

        string beamproto;

        switch (tier)
        {
            case GlimmerTier.Dangerous:
                beamproto = "SuperchargedLightning";
                break;
            case GlimmerTier.Critical:
                beamproto = "HyperchargedLightning";
                break;
            default:
                beamproto = "ChargedLightning";
                break;
        }


        _lightning.ShootLightning(prober, target, beamproto);
        BeamCooldown += 3f;
    }

    private void AnchorOrExplode(EntityUid uid)
    {
        if (Transform(uid).GridUid is null)
            _destructibleSystem.DestroyEntity(uid);

        if (HasComp<AnchorableComponent>(uid))
            _transformSystem.AnchorEntity(uid, Transform(uid));
    }

    private void OnMeleeThrowOnHitAttempt(Entity<SharedGlimmerReactiveComponent> ent, ref AttemptMeleeThrowOnHitEvent args)
    {
        if (_glimmerSystem.GetGlimmerTier() < GlimmerTier.Dangerous)
            return;

        args.Cancelled = true;
        args.Handled = true;

        _lightning.ShootRandomLightnings(ent, 10, 2, "SuperchargedLightning", 2, false);

        // Check if the parent of the user is alive, which will be the case if the user is an item and is being held.
        if (args.User == null)
            return;
        var zapTarget = _transformSystem.GetParentUid(args.User.Value);
        if (TryComp<MindContainerComponent>(zapTarget, out _))
            _electrocutionSystem.TryDoElectrocution(zapTarget, ent, 5, TimeSpan.FromSeconds(3), true,
                ignoreInsulation: true);
    }

    private void Reset(RoundRestartCleanupEvent args)
    {
        Accumulator = 0;

        // It is necessary that the GlimmerTier is reset to the default
        // tier on round restart. This system will persist through
        // restarts, and an undesired event will fire as a result after the
        // start of the new round, causing modulatable PointLights to have
        // negative Energy if the tier was higher than Minimal on restart.
        LastGlimmerTier = GlimmerTier.Minimal;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        Accumulator += frameTime;
        BeamCooldown = Math.Max(0, BeamCooldown - frameTime);

        if (Accumulator > UpdateFrequency)
        {
            var currentGlimmerTier = _glimmerSystem.GetGlimmerTier();

            var reactives = EntityQuery<SharedGlimmerReactiveComponent>();
            if (currentGlimmerTier != LastGlimmerTier)
            {
                var glimmerTierDelta = (int) currentGlimmerTier - (int) LastGlimmerTier;
                var ev = new GlimmerTierChangedEvent(LastGlimmerTier, currentGlimmerTier, glimmerTierDelta);

                foreach (var reactive in reactives)
                {
                    UpdateEntityState(reactive.Owner, reactive, currentGlimmerTier, glimmerTierDelta);
                    RaiseLocalEvent(reactive.Owner, ev);
                }

                LastGlimmerTier = currentGlimmerTier;
            }
            if (currentGlimmerTier == GlimmerTier.Critical)
            {
                _ghostSystem.MakeVisible(true);
                _revenantSystem.MakeVisible(true);
                GhostsVisible = true;
                foreach (var reactive in reactives)
                    BeamRandomNearProber(reactive.Owner, 1, 12);
            }
            else if (GhostsVisible == true)
            {
                _ghostSystem.MakeVisible(false);
                _revenantSystem.MakeVisible(false);
                GhostsVisible = false;
            }
            Accumulator = 0;
        }
    }
}

/// <summary>
/// This event is fired when the broader glimmer tier has changed,
/// not on every single adjustment to the glimmer count.
///
/// <see cref="GlimmerSystem.GetGlimmerTier"/> has the exact
/// values corresponding to tiers.
/// </summary>
public class GlimmerTierChangedEvent : EntityEventArgs
{
    /// <summary>
    /// What was the last glimmer tier before this event fired?
    /// </summary>
    public readonly GlimmerTier LastTier;

    /// <summary>
    /// What is the current glimmer tier?
    /// </summary>
    public readonly GlimmerTier CurrentTier;

    /// <summary>
    /// What is the change in tiers between the last and current tier?
    /// </summary>
    public readonly int TierDelta;

    public GlimmerTierChangedEvent(GlimmerTier lastTier, GlimmerTier currentTier, int tierDelta)
    {
        LastTier = lastTier;
        CurrentTier = currentTier;
        TierDelta = tierDelta;
    }
}
