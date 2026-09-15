using System.Linq;
using System.Text;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Goobstation.Construction;

[TestFixture]
public sealed class ConstructionPackTest
{
    [Test]
    public async Task TestAllRecipesAreInPacks()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {

        });

        var server = pair.Server;

        var protoMan = server.ResolveDependency<IPrototypeManager>();

        var errors = new StringBuilder();
        var failed = false;
        await server.WaitPost(() =>
        {
            var packs = protoMan.EnumeratePrototypes<ConstructionPackPrototype>().ToArray();
            var recipes = protoMan.EnumeratePrototypes<ConstructionPrototype>().ToArray();

            foreach (var recipe in recipes)
            {
                var passed = false;
                foreach (var pack in packs)
                {
                    if (!pack.Recipes.Contains(recipe))
                        continue;

                    passed = true;
                    break;
                }

                if (passed)
                    continue;

                errors.AppendLine(
                    $"{nameof(ConstructionPrototype)} {recipe.ID} is not in any {nameof(ConstructionPackPrototype)}!");
                failed = true;
            }
        });

        if (failed)
            Assert.Fail(errors.ToString());

        await pair.CleanReturnAsync();
    }
}
