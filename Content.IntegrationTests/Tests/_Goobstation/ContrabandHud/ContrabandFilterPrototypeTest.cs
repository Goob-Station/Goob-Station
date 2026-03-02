using System.Linq;
using Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;
using Content.Shared.Contraband;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Goobstation.ContrabandHud;

[TestFixture]
public sealed class ContraprototypeTest
{
    public sealed class CheckIfAllAreInFilterTest
    {
        [Test]
        public async Task TestAllContraInFilter()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var protoMan = server.ResolveDependency<IPrototypeManager>();
            
            await server.WaitAssertion(() =>
            {
                var filter = protoMan.Index<ContrabandFilterPrototype>("ContrabandFilter");
                var allContraprototypes = protoMan.EnumeratePrototypes<ContrabandSeverityPrototype>();
                var filtersTotal = protoMan.EnumeratePrototypes<ContrabandFilterPrototype>();
                Assert.Multiple(() =>
                {
                    Assert.That(filtersTotal.Count() == 1, Is.True,
                        $"There are multiple ContrabandFilter prototypes defined. Expected only one 'ContrabandFilter' prototype.");
                    
                    foreach (var contraProto in allContraprototypes)
                    {
                        Assert.That(filter.BlacklistedSeverity.Contains(contraProto.ID) || 
                                    filter.WhitelistedSeverity.Contains(contraProto.ID) ||
                                    filter.RequiresPermitSeverity.Contains(contraProto.ID),
                            Is.True,
                            $"Contraband prototype '{contraProto.ID}' is not included in the 'ContrabandFilter' prototype.");
                        
                        Assert.That(IsOnceInFilter(filter, contraProto.ID), Is.True,
                            $"Contraband prototype '{contraProto.ID}' is included in multiple categories in the 'ContrabandFilter' prototype.");
                    }
                    
                });
            });

            await pair.CleanReturnAsync();
        }
    }
    private static bool IsOnceInFilter(ContrabandFilterPrototype filter, ProtoId<ContrabandSeverityPrototype> protoId)
    {
        var count = 0;
        
        if (filter.BlacklistedSeverity.Contains(protoId))
            count++;
        
        if (filter.WhitelistedSeverity.Contains(protoId))
            count++;
        
        if (filter.RequiresPermitSeverity.Contains(protoId))
            count++;
        
        return count == 1;
    }
}
