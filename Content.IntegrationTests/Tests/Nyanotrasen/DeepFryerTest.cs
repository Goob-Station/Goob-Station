#nullable enable annotations
using Content.Server.Kitchen.Components;
using Content.Server.Kitchen.EntitySystems;
using Content.Server.Nyanotrasen.Kitchen.Components;
using Content.Server.Nyanotrasen.Kitchen.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.Shared.Reflection;

namespace Content.IntegrationTests.Tests.DeepFryer
{
    [TestFixture]
    [TestOf(typeof(DeepFriedComponent))]
    [TestOf(typeof(DeepFryerSystem))]
    [TestOf(typeof(DeepFryerComponent))]
    public sealed class DeepFryerTest
    {

        [TestPrototypes]
        private const string Prototypes = @"
- type: entity
  name: DeepFryerDummy
  id: DeepFryerDummy
  components:
  - type: DeepFryer
    entryDelay: 0
    draggedEntryDelay: 0
    flushTime: 0
  - type: Anchorable
  - type: ApcPowerReceiver
  - type: Physics
    bodyType: Static
  - type: Fixtures
    fixtures:
      fix1:
        shape:
          !type:PhysShapeCircle
          radius: 0.35
";

        [Test]
        public async Task Test()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var testMap = await pair.CreateTestMap();

            EntityUid unitUid = default;

            var entityManager = server.ResolveDependency<IEntityManager>();
            var xformSystem = entityManager.System<SharedTransformSystem>();
            var deepFryerSystem = entityManager.System<DeepFryerSystem>();
            await server.WaitAssertion(() =>
            {
                Assert.That(deepFryerSystem, Is.Not.Null);
            });
            await pair.CleanReturnAsync();
        }
    }
}
