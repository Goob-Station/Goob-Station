using System.Numerics;
using Content.Goobstation.Common.Jetpack;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Jittering;
using Content.Shared.Movement.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Jetpack;

public sealed class GoobJetpackSystem : CommonGoobJetpackSystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<JetpackUserComponent> _jetpackUserQuery;
    private EntityQuery<GoobJetpackComponent> _goobJetpackQuery;

    public override void Initialize()
    {
        base.Initialize();
        _jetpackUserQuery = GetEntityQuery<JetpackUserComponent>();
        _goobJetpackQuery = GetEntityQuery<GoobJetpackComponent>();
    }

    public override bool TryOverrideWishDir(EntityUid uid, out Vector2 wishDir)
    {
        wishDir = Vector2.Zero;

        if (!_jetpackUserQuery.TryComp(uid, out var jetpackUser))
            return false;

        if (!_goobJetpackQuery.TryComp(jetpackUser.Jetpack, out var goobJetpack))
            return false;

        if (!_hands.IsHolding(uid, jetpackUser.Jetpack))
            return false;

        var tick = (float) _timing.CurTick.Value;
        var seed = uid.Id * 7.13f;
        var baseAngle = tick * goobJetpack.HandScatterFrequency + seed;
        var wobble = MathF.Sin(tick * goobJetpack.HandScatterFrequency * 30f + seed * 2.3f) * goobJetpack.HandScatterWobble;
        var angle = baseAngle + wobble;
        wishDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

        _jittering.DoJitter(uid, TimeSpan.FromSeconds(0.4), true, goobJetpack.HandJitterAmplitude, goobJetpack.HandJitterFrequency);
        return true;
    }
}
