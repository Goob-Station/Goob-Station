// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Vox;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Goobstation.VoxAudio;

[TestFixture]
public sealed class VoxAudioTest
{

    /// <summary>
    /// Enforce that every word has an explicit path if there is no BasePath provided.
    /// </summary>
    [Test]
    public async Task VoxVoicePrototypeBasePathTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            foreach (var voice in protoMan.EnumeratePrototypes<VoxVoicePrototype>())
            {
                if (voice.BasePath != null)
                    continue;

                foreach (var word in voice.Words)
                {
                    Assert.That(word.Path, Is.Not.Null, $"VoxVoice '{voice}' had no BasePath but contained VoxWord '{word}' which had no explicit Path!");
                }
            }
        });

        await pair.CleanReturnAsync();
    }
}
