using Robust.Shared.Prototypes;
using Robust.Shared.Localization;
using Content.Shared.Language;
using System.Linq;
using System.Collections.Generic;

namespace Content.IntegrationTests.Tests.Traits;

/// <summary>
///    Checks if every language has a valid name, chatname, and description localization string.
/// </summary>
[TestFixture]
[TestOf(typeof(LanguagePrototype))]
public sealed class LanguageLocalizationTest
{
    [Test]
    public async Task TestLanguageLocalization()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var locale = server.ResolveDependency<ILocalizationManager>();
        var proto = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var missingStrings = new List<string>();

            foreach (var langProto in proto.EnumeratePrototypes<LanguagePrototype>().OrderBy(a => a.ID))
                foreach (var locString in new List<string> { $"language-{langProto.ID}-name", $"chat-language-{langProto.ID}-name", $"language-{langProto.ID}-description" })
                    if (!locale.HasString(locString))
                        missingStrings.Add($"\"{langProto.ID}\", \"{locString}\"");

            Assert.That(!missingStrings.Any(), Is.True, $"The following languages are missing localization strings:\n  {string.Join("\n  ", missingStrings)}");
        });

        await pair.CleanReturnAsync();
    }
}
