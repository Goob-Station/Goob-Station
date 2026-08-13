#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Content.Goobstation.Client.Antag.Intro;
using Content.Goobstation.Shared.Antag.Intro;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;
using Robust.Shared.Sandboxing;
using Robust.Shared.Timing;

namespace Content.IntegrationTests.Tests.Goobstation;

/// <summary>
/// Stages every antagonist opening on a real client and walks each one past the end of its own
/// runtime, so the client-side map it builds, every entity in it and every cue it fires get
/// exercised - and so each one is provably gone again afterwards.
/// </summary>
[TestFixture]
[TestOf(typeof(AntagIntroScene))]
public sealed class AntagIntroTest
{
    [Test]
    public async Task TestAllScenesRunAndCleanUp()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var client = pair.Client;
        var ui = client.ResolveDependency<IUserInterfaceManager>();
        var mapSys = client.System<SharedMapSystem>();
        var protoMan = client.ResolveDependency<IPrototypeManager>();
        var reflection = client.ResolveDependency<IReflectionManager>();
        var sandbox = client.ResolveDependency<ISandboxHelper>();
        var frameUpdate = typeof(Control).GetMethod("FrameUpdate", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var scenes = new Dictionary<string, Type>();

        await client.WaitAssertion(() =>
        {
            foreach (var type in reflection.GetAllChildren<AntagIntroScene>())
                scenes.Add(type.Name, type);

            foreach (var intro in protoMan.EnumeratePrototypes<AntagIntroPrototype>())
            {
                Assert.That(scenes.ContainsKey(intro.Scene), Is.True,
                    $"Antag intro {intro.ID} names unknown scene {intro.Scene}.");
            }
        });

        foreach (var (name, type) in scenes)
        {
            AntagIntroScene? intro = null;
            var maps = 0;

            await client.WaitAssertion(() =>
            {
                maps = mapSys.GetAllMapIds().Count();
                intro = (AntagIntroScene) sandbox.CreateInstance(type);
                ui.WindowRoot.AddChild(intro);

                Assert.That(mapSys.GetAllMapIds().Count(), Is.GreaterThan(maps),
                    $"{name} did not build itself a client-side map.");
            });

            await client.WaitAssertion(() =>
            {
                for (var i = 0; i < 500 && !intro!.Finished; i++)
                    frameUpdate.Invoke(intro, new object?[] { new FrameEventArgs(0.04f) });

                Assert.That(intro!.Finished, Is.True, $"{name} never finished.");
            });

            await client.WaitAssertion(() =>
            {
                intro!.Orphan();

                Assert.That(mapSys.GetAllMapIds().Count(), Is.EqualTo(maps),
                    $"{name} left its map behind.");
            });
        }

        await pair.CleanReturnAsync();
    }
}
