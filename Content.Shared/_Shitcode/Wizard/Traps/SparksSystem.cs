using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Goobstation.Wizard.Traps;

public sealed class SparksSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private static readonly EntProtoId SparkPrototype = "EffectSpark";

    private static readonly SoundSpecifier Sound = new SoundCollectionSpecifier("sparks");

    public void DoSparks(EntityCoordinates coords,
        int minSparks = 3,
        int maxSparks = 6,
        float minVelocity = 1f,
        float maxVelocity = 4f,
        bool playSound = true)
    {
        if (_net.IsClient)
            return;

        var amount = _random.Next(minSparks, maxSparks + 1);

        if (amount <= 0)
            return;

        if (playSound)
            _audio.PlayPvs(Sound, coords);

        var mapCoords = _transform.ToMapCoordinates(coords);

        float? velocityOverride = minVelocity < maxVelocity ? null : minVelocity;

        for (var i = 0; i < amount; i++)
        {
            var velocity = velocityOverride ?? _random.NextFloat(minVelocity, maxVelocity);
            var dir = _random.NextAngle().ToVec() * velocity;
            var spark = Spawn(SparkPrototype, mapCoords);
            _physics.SetLinearVelocity(spark, dir);
        }
    }
}
