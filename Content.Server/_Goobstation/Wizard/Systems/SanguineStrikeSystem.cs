using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Goobstation.Wizard.SanguineStrike;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class SanguineStrikeSystem : SharedSanguineStrikeSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PointLightSystem _light = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _bloodStream = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanguineStrikeComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SanguineStrikeComponent, ComponentRemove>(OnRemove);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SanguineStrikeComponent>();
        while (query.MoveNext(out var uid, out var sanguine))
        {
            sanguine.Lifetime -= frameTime;

            if (sanguine.Lifetime <= 0)
                RemCompDeferred(uid, sanguine);
        }
    }

    private void OnRemove(Entity<SanguineStrikeComponent> ent, ref ComponentRemove args)
    {
        var (uid, comp) = ent;

        if (TerminatingOrDeleted(uid))
            return;

        _popup.PopupEntity(Loc.GetString("sanguine-strike-end", ("item", uid)), uid);

        if (comp.HadPointLight)
            return;

        RemComp<PointLightComponent>(uid);
    }

    private void OnInit(Entity<SanguineStrikeComponent> ent, ref ComponentInit args)
    {
        var (uid, comp) = ent;

        if (HasComp<PointLightComponent>(uid))
        {
            comp.HadPointLight = true;
            return;
        }

        var light = _light.EnsureLight(uid);
        _light.SetRadius(uid, comp.LightRadius, light);
        _light.SetEnergy(uid, comp.LightEnergy, light);
        _light.SetColor(uid, comp.Color, light);
    }

    protected override void Hit(EntityUid uid,
        SanguineStrikeComponent component,
        EntityUid user,
        IReadOnlyList<EntityUid> hitEntities)
    {
        base.Hit(uid, component, user, hitEntities);

        var bloodQuery = GetEntityQuery<BloodstreamComponent>();
        var solutionQuery = GetEntityQuery<SolutionContainerManagerComponent>();

        var xform = Transform(uid);

        // I love solutions :)
        if (bloodQuery.TryComp(user, out var userBlood) && solutionQuery.TryComp(user, out var userSolution) &&
            _solution.ResolveSolution((user, userSolution), userBlood.BloodSolutionName, ref userBlood.BloodSolution))
        {
            List<Entity<BloodstreamComponent, SolutionContainerManagerComponent>> bloodEntities = new();
            foreach (var hitEnt in hitEntities)
            {
                if (bloodQuery.TryComp(hitEnt, out var hitBlood) && solutionQuery.TryComp(hitEnt, out var hitSolution))
                    bloodEntities.Add((hitEnt, hitBlood, hitSolution));
            }

            if (bloodEntities.Count > 0)
            {
                Solution tempSol = new();
                var missingBlood = userBlood.BloodMaxVolume - userBlood.BloodSolution.Value.Comp.Solution.Volume;
                missingBlood = FixedPoint2.Max(FixedPoint2.Zero, missingBlood);
                var bloodSuckAmount = component.BloodSuckAmount / bloodEntities.Count;
                foreach (var (entity, blood, solution) in bloodEntities)
                {
                    if (!_solution.ResolveSolution((entity, solution),
                            blood.BloodSolutionName,
                            ref blood.BloodSolution))
                        continue;

                    var bloodToRemove = FixedPoint2.Min(blood.BloodSolution.Value.Comp.Solution.Volume,
                        bloodSuckAmount);
                    tempSol.MaxVolume += bloodToRemove;
                    tempSol.AddSolution(_solution.SplitSolution(blood.BloodSolution.Value, bloodToRemove), _proto);
                }

                var restoredBlood = FixedPoint2.Min(tempSol.Volume, missingBlood);
                _bloodStream.TryModifyBloodLevel(user, restoredBlood, userBlood);
                _bloodStream.TryModifyBleedAmount(user, -userBlood.BleedAmount, userBlood);
                if (restoredBlood < tempSol.Volume && tempSol.Volume > 0 && tempSol.Contents.Count > 0)
                {
                    var toRemove = restoredBlood / tempSol.Volume;
                    for (var i = tempSol.Contents.Count - 1; i >= 0; i--)
                    {
                        tempSol.RemoveReagent(tempSol.Contents[i].Reagent,
                            tempSol.Contents[i].Quantity * toRemove,
                            true,
                            true);
                    }

                    _puddle.TrySpillAt(xform.Coordinates, tempSol, out _);
                }
            }
        }

        _audio.PlayPvs(component.LifestealSound, xform.Coordinates);

        Spawn(component.Effect, _transform.GetMapCoordinates(xform));

        RemCompDeferred(uid, component);
    }
}
