using System.Linq;
using Content.Server._MACRO.StrangeMoods;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared.Dataset;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._MACRO.StrangeMoods;

[TestFixture, TestOf(typeof(StrangeMoodPrototype))]
public sealed class StrangeMoodTests
{
    private const string ThreeValueSet = "ThreeValueSet";
    private const string DuplicateTest = "DuplicateTest";
    private const string DuplicateOverlapTest = "DuplicateOverlapTest";

    [TestPrototypes]
    private const string Prototypes = """

        - type: dataset
          id: ThreeValueSet
          values:
            - One
            - Two
            - Three
        - type: strangeMood
          id: DuplicateTest
          moodName: DuplicateTest
          moodDesc: DuplicateTest
          allowDuplicateMoodVars: false
          moodVars:
            a: ThreeValueSet
            b: ThreeValueSet
            c: ThreeValueSet
        - type: strangeMood
          id: DuplicateOverlapTest
          moodName: DuplicateOverlapTest
          moodDesc: DuplicateOverlapTest
          allowDuplicateMoodVars: false
          moodVars:
            a: ThreeValueSet
            b: ThreeValueSet
            c: ThreeValueSet
            d: ThreeValueSet
            e: ThreeValueSet

        """;

    [Test]
    [Repeat(10)]
    public async Task TestDuplicatePrevention()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        await server.WaitIdleAsync();

        var entMan = server.ResolveDependency<IEntityManager>();
        var moodSystem = entMan.System<StrangeMoodsSystem>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        var dataset = protoMan.Index<DatasetPrototype>(ThreeValueSet);
        var moodProto = protoMan.Index<StrangeMoodPrototype>(DuplicateTest);

        var datasetSet = dataset.Values.ToHashSet();
        var mood = moodSystem.RollMood(moodProto);
        var moodVarSet = mood.MoodVars.Values.ToHashSet();

        Assert.That(moodVarSet, Is.EquivalentTo(datasetSet));

        await pair.CleanReturnAsync();
    }

    [Test]
    [Repeat(10)]
    public async Task TestDuplicateOverlap()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var entMan = server.ResolveDependency<IEntityManager>();
        var moodSystem = entMan.System<StrangeMoodsSystem>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        var dataset = protoMan.Index<DatasetPrototype>(ThreeValueSet);
        var moodProto = protoMan.Index<StrangeMoodPrototype>(DuplicateOverlapTest);

        var datasetSet = dataset.Values.ToHashSet();
        var mood = moodSystem.RollMood(moodProto);
        var moodVarSet = mood.MoodVars.Values.ToHashSet();

        Assert.That(moodVarSet, Is.EquivalentTo(datasetSet));

        await pair.CleanReturnAsync();
    }
}