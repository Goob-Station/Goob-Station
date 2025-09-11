using Robust.Shared.Random;

namespace Content.IntegrationTests.Tests._Goobstation;

/// <summary>
/// On purpose literally random test fail
/// </summary>
[TestFixture]
public sealed class GoidaTest
{
    private const float _chance = 0.065f;

    [Test]
    public async Task ValidateChaosScores()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var random = server.ResolveDependency<IRobustRandom>();
        await server.WaitAssertion(() =>
        {
            Assert.That(random.NextFloat() < _chance, "curse of 220");
        });

        await pair.CleanReturnAsync();
    }
}
