using Content.Shared.Mind;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server._CorvaxGoob.Store.Conditions;

public sealed partial class ObjectiveCondition : ListingCondition
{
    /// <summary>
    /// A whitelist of objectives that can allow purchase this listing. Only one needs to be found.
    /// </summary>
    [DataField("whitelist", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EntityPrototype>))]
    public HashSet<string>? Whitelist;

    /// <summary>
    /// A blacklist of objectives that can block purchase this listing. Only one needs to be found.
    /// </summary>
    [DataField("blacklist", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EntityPrototype>))]
    public HashSet<string>? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;

        if (!ent.TryGetComponent<MindComponent>(args.Buyer, out var mind))
            return true; // inanimate objects don't have minds

        var objectives = mind.Objectives;


        if (Blacklist is not null)
        {
            foreach (var objective in objectives)
            {
                if (!ent.TryGetComponent<MetaDataComponent>(objective, out var meta))
                    continue;

                if (meta.EntityPrototype is not null && Blacklist.Contains(meta.EntityPrototype.ID))
                    return false;
            }
        }

        if (Whitelist is not null)
        {
            var found = false;
            foreach (var objective in objectives)
            {
                if (!ent.TryGetComponent<MetaDataComponent>(objective, out var meta))
                    continue;

                if (meta.EntityPrototype is not null && Whitelist.Contains(meta.EntityPrototype.ID))
                    found = true;
            }
            if (!found)
                return false;
        }

        return true;
    }
}
