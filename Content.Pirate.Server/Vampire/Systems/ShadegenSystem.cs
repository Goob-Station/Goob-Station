using Content.Pirate.Server.Vampire.Components;
using Content.Pirate.Shared.Vampire.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Light.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.Vampire.Systems;

public sealed partial class ShadegenSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HandheldLightSystem _handheldLight = default!;

    private readonly HashSet<EntityUid> _updateQueue = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShadegenComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.NextUpdate)
                continue;

            component.NextUpdate = _timing.CurTime + component.UpdateCooldown;

            foreach (var toUpdate in _updateQueue)
            {
                if (Deleted(toUpdate))
                    continue;

                RemComp<ShadegenAffectedComponent>(toUpdate);
            }

            _updateQueue.Clear();

            var lightQuery = _lookup.GetEntitiesInRange<PointLightComponent>(Transform(uid).Coordinates, component.Range);
            foreach (var light in lightQuery)
            {
                if (HasComp<DarkLightComponent>(light.Owner))
                    continue;

                EnsureComp<ShadegenAffectedComponent>(light.Owner);
                _updateQueue.Add(light.Owner);

                if (TryComp<HandheldLightComponent>(light.Owner, out var handheldcomp) && handheldcomp.Activated)
                    _handheldLight.TurnOff((light.Owner, handheldcomp), makeNoise: false);

                if (!component.DestroyLights ||
                    !TryComp<PoweredLightComponent>(light.Owner, out var poweredcomp) ||
                    !poweredcomp.On)
                    continue;

                // Pirate: Starlight also raises a railroading light-break event; that subsystem is not present here.
                _light.TryDestroyBulb(light.Owner, poweredcomp);
            }
        }
    }
}
