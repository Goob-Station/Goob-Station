using Content.Shared.EntityConditions;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Tools;
using Content.Shared.Tools.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class CarryingEntityConditionSystem : EntityConditionSystem<HandsComponent, CarryingCondition>
{
    [Dependency] private CarriedItemsSystem _carried = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    protected override void Condition(Entity<HandsComponent> entity, ref EntityConditionEvent<CarryingCondition> args)
    {
        foreach (var item in _carried.GetCarriedItems(entity))
        {
            if (!_whitelist.IsWhitelistPass(args.Condition.Whitelist, item))
                continue;

            args.Result = true;
            return;
        }
    }
}

public sealed partial class CarryingToolsEntityConditionSystem : EntityConditionSystem<HandsComponent, CarryingToolsCondition>
{
    [Dependency] private CarriedItemsSystem _carried = default!;
    [Dependency] private SharedToolSystem _tool = default!;

    protected override void Condition(Entity<HandsComponent> entity, ref EntityConditionEvent<CarryingToolsCondition> args)
    {
        var missing = new HashSet<ProtoId<ToolQualityPrototype>>(args.Condition.Qualities);
        foreach (var item in _carried.GetCarriedItems(entity))
        {
            missing.RemoveWhere(quality => _tool.HasQuality(item, quality));
            if (missing.Count == 0)
                break;
        }

        args.Result = missing.Count == 0;
    }
}

public sealed partial class CarriedItemsSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventory = default!;

    private const int MaxDepth = 3;

    public List<EntityUid> GetCarriedItems(EntityUid user)
    {
        var items = new List<EntityUid>();
        foreach (var item in _inventory.GetHandOrInventoryEntities(user))
        {
            Collect(item, items, 0);
        }

        return items;
    }

    private void Collect(EntityUid item, List<EntityUid> items, int depth)
    {
        items.Add(item);

        if (depth >= MaxDepth || !TryComp<ContainerManagerComponent>(item, out var manager))
            return;

        foreach (var container in manager.Containers.Values)
        {
            foreach (var contained in container.ContainedEntities)
            {
                Collect(contained, items, depth + 1);
            }
        }
    }
}

public sealed partial class CarryingCondition : EntityConditionBase<CarryingCondition>
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}

public sealed partial class CarryingToolsCondition : EntityConditionBase<CarryingToolsCondition>
{
    [DataField(required: true)]
    public List<ProtoId<ToolQualityPrototype>> Qualities = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
