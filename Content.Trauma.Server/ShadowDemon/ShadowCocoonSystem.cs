using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Light.Components;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Content.Trauma.Shared.ShadowDemon.ShadowCocoon;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.ShadowDemon;

public sealed class ShadowCocoonSystem : SharedShadowCocoonSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;

    private EntityQuery<ExpendableLightComponent> _expendableLightQuery;
    private EntityQuery<WelderComponent> _welderQuery;
    private EntityQuery<PoweredLightComponent> _poweredLightQuery;

    private readonly HashSet<Entity<PointLightComponent>> _pointLights = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _expendableLightQuery = GetEntityQuery<ExpendableLightComponent>();
        _welderQuery = GetEntityQuery<WelderComponent>();
        _poweredLightQuery = GetEntityQuery<PoweredLightComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var eqe = EntityQueryEnumerator<ShadowCocoonComponent>();
        while (eqe.MoveNext(out var uid, out var cocoon))
        {
            if (now < cocoon.NextUpdate)
                return;

            _pointLights.Clear();
            _lookup.GetEntitiesInRange(Transform(uid).Coordinates, cocoon.Radius, _pointLights);
            foreach (var entity in _pointLights)
            {
                if (!entity.Comp.Enabled)
                    continue;

                var owner = entity.Owner;

                if (_poweredLightQuery.TryComp(entity, out var poweredLight) && poweredLight.On)
                {
                    // Destroy nearby light bulbs
                    _poweredLight.TryDestroyBulb(entity, poweredLight);
                    continue;
                }

                if (_welderQuery.TryComp(entity, out var welder) && welder.Enabled)
                {
                    // Remove all fuel from the welder
                    if (!_solutionContainer.TryGetSolution(owner, welder.FuelSolutionName, out var solution))
                        return;
                    var fuel = _tool.GetWelderFuelAndCapacity(entity, welder);

                    _solutionContainer.RemoveReagent(solution.Value, welder.FuelReagent, fuel.fuel);

                    _tool.TurnOff((entity, welder), uid);
                    continue;
                }

                if (_expendableLightQuery.TryComp(entity, out var expandable))
                {
                    // Kill flare stuff
                    expandable.CurrentState = ExpendableLightState.Fading;
                    expandable.StateExpiryTime = 0;
                    Dirty(entity, expandable);
                }
            }
        }
    }
}
