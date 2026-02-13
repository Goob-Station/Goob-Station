#nullable enable
using System.Numerics;
using Content.Goobstation.Common.MomentumSteering;
using Content.IntegrationTests.Pair;

namespace Content.IntegrationTests.Tests.Goobstation;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class MomentumSteeringTest
{
    private TestPair _pair = default!;
    private CommonMomentumSteeringSystem _sys = default!;
    private MomentumSteeringComponent _comp = default!;

    [SetUp]
    public async Task Setup()
    {
        _pair = await PoolManager.GetServerClient();
        _sys = _pair.Server.System<CommonMomentumSteeringSystem>();
        _comp = new MomentumSteeringComponent();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestBelowThresholdNoSteering()
    {
        await _pair.Server.WaitAssertion(() =>
        {
            var velocity = new Vector2(2f, 0f);
            var wishDir = new Vector2(0f, 1f);

            var result = _sys.TryAdjustedWishDir(_comp, velocity, wishDir, out var adjusted, out _);

            Assert.That(result, Is.False, "Should return false below speed threshold");
            Assert.That(adjusted, Is.EqualTo(wishDir), "WishDir should be unchanged below threshold");
        });
    }

    [Test]
    public async Task TestPerpPenalized()
    {
        await _pair.Server.WaitAssertion(() =>
        {
            var velocity = new Vector2(_comp.MaxSpeed, 0f);
            var wishDir = Vector2.Normalize(new Vector2(1f, 1f));

            var result = _sys.TryAdjustedWishDir(_comp, velocity, wishDir, out var adjusted, out _);

            Assert.That(result, Is.True);
            Assert.That(adjusted.X, Is.EqualTo(wishDir.X).Within(0.01f),
                "Forward component should be unpenalized");
            Assert.That(Math.Abs(adjusted.Y), Is.LessThan(wishDir.Y * 0.2f),
                "Perpendicular component should be heavily penalized at max speed");
        });
    }

    [Test]
    public async Task TestBrakingFactor()
    {
        await _pair.Server.WaitAssertion(() =>
        {
            var velocity = new Vector2(_comp.MaxSpeed, 0f);
            var wishDir = new Vector2(-1f, 0f);

            var result = _sys.TryAdjustedWishDir(_comp, velocity, wishDir, out var adjusted, out _);

            Assert.That(result, Is.True);
            Assert.That(adjusted.X, Is.EqualTo(-_comp.BrakingFactor).Within(0.01f),
                "Braking force should be scaled by BrakingFactor");
            Assert.That(Math.Abs(adjusted.Y), Is.LessThan(0.01f),
                "No perpendicular component when braking straight back");
        });
    }

    [Test]
    public async Task TestBelowThresholdUnchanged()
    {
        await _pair.Server.WaitAssertion(() =>
        {
            var friction = 1.0f;
            _sys.KillFriction(_comp, new Vector2(2f, 0f), ref friction);
            Assert.That(friction, Is.EqualTo(1.0f), "Friction should be unchanged below speed threshold");
        });
    }

    [Test]
    public async Task TestMaxSpeedReduced()
    {
        await _pair.Server.WaitAssertion(() =>
        {
            var friction = 1.0f;
            _sys.KillFriction(_comp, new Vector2(_comp.MaxSpeed, 0f), ref friction);
            Assert.That(friction, Is.EqualTo(_comp.FrictionReductionAtSpeed).Within(0.01f),
                "Friction should equal FrictionReductionAtSpeed at max speed");
        });
    }

    [Test]
    public async Task TestMaxSpeedDiagBrakePenalized()
    {
        await _pair.Server.WaitAssertion(() =>
        {
            var velocity = new Vector2(_comp.MaxSpeed, 0f);
            var wishDir = Vector2.Normalize(new Vector2(-1f, -1f));

            var result = _sys.TryAdjustedWishDir(_comp, velocity, wishDir, out var adjusted, out _);

            Assert.That(result, Is.True);

            // forwardDir = (1,0) * dot(wishDir, (1,0)) = (wishDir.X, 0)
            // perpDir = (0, wishDir.Y)
            var expectedX = wishDir.X * _comp.BrakingFactor;
            var expectedY = wishDir.Y * _comp.BrakingFactor;

            Assert.That(adjusted.X, Is.EqualTo(expectedX).Within(0.01f),
                "Reverse component should use BrakingFactor");
            Assert.That(adjusted.Y, Is.EqualTo(expectedY).Within(0.01f),
                "Perpendicular component should use BrakingFactor when not pressing forward");
        });
    }
}
