using Content.Pirate.Shared.BurnableByThermite;
using Content.Server.Destructible;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.IgnitionSource;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.BurnableByThermite;

public sealed partial class BurnableByThermiteSystem : SharedBurnableByThermiteSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DestructibleSystem _destructibleSystem = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BurnableByThermiteComponent, InteractUsingEvent>(OnInteract);
    }

    // Interaction Handling
    public void OnInteract(EntityUid uid, BurnableByThermiteComponent component, InteractUsingEvent args)
    {
        if (TryComp<SolutionContainerManagerComponent>(args.Used, out var beakerSolutionContainerComponent))
            OnInteractWithBeaker(uid, component, args.Used, args, beakerSolutionContainerComponent);
        if (TryComp<IgnitionSourceComponent>(args.Used, out var ignitionSourceComponent))
            OnInteractWithLighter(new(uid, component), args, ignitionSourceComponent);
        // TryComp<IgnitionSourceComponent>(args.Used, out var ignitionSourceComponent);
    }
    public void OnInteractWithLighter(Entity<BurnableByThermiteComponent> ent, InteractUsingEvent args, IgnitionSourceComponent ignitionSourceComponent)
    {
        if (!ignitionSourceComponent.Ignited) return;
        if (ent.Comp.Ignited || ent.Comp.Burning) return;
        TryIgniteStructure(ent, args);
    }
    public void OnInteractWithBeaker(EntityUid structure, BurnableByThermiteComponent component, EntityUid beaker, InteractUsingEvent args, SolutionContainerManagerComponent beakerSolutionContainerComponent)
    {
        if (beakerSolutionContainerComponent.Containers is null) return;
        if (beakerSolutionContainerComponent.Containers.Count == 0) return;

        foreach (var (_, solutionEntity) in _solutionSystem.EnumerateSolutions(beaker))
        {
            if (!solutionEntity.Comp.Solution.TryGetReagent(new ReagentId("Thermite", null), out var thermiteReagent))
                continue;
            if (thermiteReagent.Quantity < component.ThermiteAmout)
            {
                _popupSystem.PopupEntity(Loc.GetString("thermite-on-structure-not-enough"), args.User, args.User, PopupType.Small);
                continue;
            }
            else
            {
                component.CoveredInThermite = true;
                _popupSystem.PopupEntity(Loc.GetString("thermite-on-structure-success"), args.User, args.User, PopupType.MediumCaution);
                _solutionSystem.RemoveReagent(solutionEntity, "Thermite", component.ThermiteAmout);
                SetSpriteData(structure, BurnableByThermiteVisuals.CoveredInThermite, true);
            }
        }

    }

    // Update
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<BurnableByThermiteComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Ignited && !component.Burning) continue;
            if (component.Ignited) OnUpdateIgnited(uid, component);
            if (component.Burning) OnUpdateBurning(uid, component);
        }
    }
    public void OnUpdateBurning(EntityUid uid, BurnableByThermiteComponent component)
    {
        SetSpriteData(uid, BurnableByThermiteVisuals.OnFireFull, true);
        if (_gameTiming.CurTime.TotalSeconds - component.BurningStartTime >= component.BurnTime)
        {
            component.TotalDamageDealt = component.DPS * component.BurnTime;
            if (component.TotalDamageDealt >= component.TotalDamageUntilMelting)
                OnEntityBurned(uid, component);

            TryExtinguishStructure(new(uid, component));
            return;
        }
        if (component.TotalDamageDealt >= component.TotalDamageUntilMelting)
        {
            OnEntityBurned(uid, component);
            return;
        }
    }
    public void OnUpdateIgnited(EntityUid uid, BurnableByThermiteComponent component)
    {
        if (_gameTiming.CurTime.TotalSeconds - component.IgnitionStartTime >= component.IgnitionTime)
        {
            component.Ignited = false;
            component.Burning = true;
            SetLightningState(uid, true, 3f);
            component.BurningSound.Params.AddVolume(2f);
            component.BurningStartTime = (float) _gameTiming.CurTime.TotalSeconds;
        }
    }

    // Secondary Media
    public void SetSpriteData(EntityUid uid, Enum visuals, bool state, bool disableOthers = false)
    {
        if (_appearanceSystem.TryGetData<bool>(uid, visuals, out var currentState))
            if (currentState == state) return;

        if (disableOthers)
        {
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.CoveredInThermite, false);
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.OnFireStart, false);
            _appearanceSystem.SetData(uid, BurnableByThermiteVisuals.OnFireFull, false);
        }
        _appearanceSystem.SetData(uid, visuals, state);
    }
    public bool TryPlayBurningSound(EntityUid uid, BurnableByThermiteComponent component)
    {
        StopPlayingBurningSound(component);
        var resolvedSound = _audioSystem.ResolveSound(component.BurningSound);

        component.BurningSoundStream = _audioSystem.PlayPvs(resolvedSound, uid)?.Entity;
        return true;
    }
    public void StopPlayingBurningSound(BurnableByThermiteComponent component)
    {
        if (_audioSystem.IsPlaying(component.BurningSoundStream))
            component.BurningSoundStream = _audioSystem.Stop(component.BurningSoundStream);
    }
    public void SetLightningState(EntityUid uid, bool state, float energy = 1f)
    {
        if (state)
        {
            _pointLight.EnsureLight(uid);
            _pointLight.SetColor(uid, Color.Yellow);
            _pointLight.SetEnergy(uid, energy);
        }
        else
        {
            _pointLight.RemoveLightDeferred(uid);
        }
    }

    // Secondary Structure
    public void OnEntityBurned(EntityUid uid, BurnableByThermiteComponent component)
    {
        StopPlayingBurningSound(component);
        _destructibleSystem.DestroyEntity(uid);
    }
    public bool TryExtinguishStructure(Entity<BurnableByThermiteComponent> ent)
    {
        if (!ent.Comp.Ignited && !ent.Comp.Burning) return false;
        SetSpriteData(ent.Owner, BurnableByThermiteVisuals.CoveredInThermite, false, true);
        ent.Comp.Ignited = false;
        ent.Comp.Burning = false;
        SetLightningState(ent.Owner, false);
        StopPlayingBurningSound(ent.Comp);
        return true;
    }
    public bool TryIgniteStructure(Entity<BurnableByThermiteComponent> ent, InteractUsingEvent args)
    {
        if (!ent.Comp.CoveredInThermite) return false;
        SetSpriteData(ent.Owner, BurnableByThermiteVisuals.OnFireStart, true);
        _popupSystem.PopupEntity(Loc.GetString("thermite-on-structure-ignited"), args.User, args.User, PopupType.MediumCaution);
        ent.Comp.Ignited = true;
        SetLightningState(ent.Owner, true, 1f);
        TryPlayBurningSound(ent.Owner, ent.Comp);
        ent.Comp.IgnitionStartTime = _gameTiming.CurTime.TotalSeconds;
        ent.Comp.CoveredInThermite = false;
        return true;
    }
}
