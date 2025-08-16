using System.Collections.Generic;
using System.Linq;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Goobstation.Shared.Stunnable;
using Content.Shared.Prototypes;
using Content.Shared.Tag;
using Content.Shared.VendingMachines;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Goobstation.VendingMachine;

public sealed class BlacklistedStatsInVendingMachinesTest
{
    private static readonly ProtoId<TagPrototype> NoStatModifyingItemsTag = "NoStatModifyingItems";

    /// <summary>
    /// Checks every prototype in all vending machines that should not have gear with stats
    /// to ensure they do not have stats.
    /// </summary>
    /// <remarks>
    /// Stop putting fucking jackboots where they do not belong.
    /// </remarks>
    [Test]
    public async Task ValidateNoStatsInBlacklistedMachines()
    {
        await using var pair   = await PoolManager.GetServerClient();
        var             server = pair.Server;

        var protoMan         = server.ProtoMan;
        var componentFactory = server.Resolve<IComponentFactory>();

        var vendingMachines  = pair.GetPrototypesWithComponent<VendingMachineComponent>();
        var eligibleMachines = new Dictionary<EntityPrototype, VendingMachineComponent>();

        await server.WaitAssertion(() =>
        {
            foreach (var (proto, comp) in vendingMachines)
            {
                if (!proto.TryGetComponent(out TagComponent tagComponent, componentFactory))
                    continue;

                foreach (var _ in tagComponent.Tags.Where(tag => tag == NoStatModifyingItemsTag))
                    eligibleMachines[proto] = comp;
            }

            foreach (var (machineProto, component) in eligibleMachines)
            {
                var inventory = protoMan.Index<VendingMachineInventoryPrototype>(component.PackPrototypeId);
                foreach (var (itemProto, _) in inventory.StartingInventory)
                {
                    var item = protoMan.Index(itemProto);

                    Assert.Multiple(() =>
                    {
                        Assert.That(item.HasComponent<ModifyStandingUpTimeComponent>(),
                            Is.False,
                            $"\"{itemProto}\" modifies standing up time, yet is in a blacklisted machine! ({machineProto})");
                        Assert.That(item.HasComponent<ClothingModifyStunTimeComponent>(),
                            Is.False,
                            $"\"{itemProto}\" modifies stun time, yet is in a blacklisted machine! ({machineProto})");
                    });
                }
            }
        });

    await pair.CleanReturnAsync();
    }
}
