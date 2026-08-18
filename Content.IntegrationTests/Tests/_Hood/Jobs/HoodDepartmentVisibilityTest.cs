// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Hood.Jobs;

[TestFixture]
public sealed class HoodDepartmentVisibilityTest
{
    private static readonly string[] HoodDepartments =
    {
        "Crisps",
        "Buds",
        "Retail",
        "Underground",
    };

    private static readonly string[] PreservedStationDepartments =
    {
        "Cargo",
        "Civilian",
        "CentralCommand",
        "Command",
        "Engineering",
        "Medical",
        "Security",
        "Science",
        "Silicon",
        "Specific",
    };

    [Test]
    public async Task CharacterPreferencesExposeOnlyHoodDepartments()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var prototypes = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var visible = prototypes.EnumeratePrototypes<DepartmentPrototype>()
                .Where(department => !department.EditorHidden)
                .Select(department => department.ID);

            Assert.That(visible, Is.EquivalentTo(HoodDepartments));

            foreach (var id in PreservedStationDepartments)
                Assert.That(prototypes.HasIndex<DepartmentPrototype>(id), Is.True, id);
        });

        await pair.CleanReturnAsync();
    }
}
