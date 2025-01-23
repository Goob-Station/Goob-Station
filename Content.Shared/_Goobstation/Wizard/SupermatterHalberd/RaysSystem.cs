using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Goobstation.Wizard.SupermatterHalberd;

public sealed class RaysSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public EntityUid? DoRays(MapCoordinates coords,
        Color colorA,
        Color colorB,
        int min = 5,
        int max = 10,
        Vector2? minMaxRadius = null,
        Vector2? minMaxEnergy = null,
        string proto = "EffectRay")
    {
        if (_net.IsClient || min > max)
            return null;

        var amount = _random.Next(min, max + 1);
        if (amount < 1)
            return null;

        var parent = Spawn(proto, coords, rotation: _random.NextAngle());
        RandomizeLight(parent);

        for (var i = 0; i < amount - 1; i++)
        {
            var newRay = Spawn(proto, coords, rotation: _random.NextAngle());
            _transform.SetParent(newRay, parent);
            RandomizeLight(newRay);
        }

        return parent;

        void RandomizeLight(EntityUid ray)
        {
            _pointLight.SetColor(ray, Color.InterpolateBetween(colorA, colorB, _random.NextFloat()));
            if (minMaxRadius != null && minMaxRadius.Value.X < minMaxRadius.Value.Y && minMaxRadius.Value.X >= 0)
                _pointLight.SetRadius(ray, _random.NextFloat(minMaxRadius.Value.X, minMaxRadius.Value.Y));
            if (minMaxEnergy != null && minMaxEnergy.Value.X < minMaxEnergy.Value.Y && minMaxEnergy.Value.X >= 0)
                _pointLight.SetEnergy(ray, _random.NextFloat(minMaxEnergy.Value.X, minMaxEnergy.Value.Y));
        }
    }
}
