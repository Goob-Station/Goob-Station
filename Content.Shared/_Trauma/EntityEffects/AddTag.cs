using Content.Shared.EntityEffects;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._Trauma.EntityEffects;

/// <summary>
/// Adds a tag to the target entity.
/// </summary>
public sealed partial class AddTag : EntityEffectBase<AddTag>
{
    /// <summary>
    /// Tag to add.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    /// <summary>
    /// Text to use for the guidebook entry for reagents.
    /// </summary>
    [DataField]
    public LocId? GuidebookText;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => GuidebookText is { } loc ? Loc.GetString(loc, ("chance", Probability)) : null;
}

public sealed partial class AddTagEffectSystem : EntityEffectSystem<TagComponent, AddTag>
{
    [Dependency] private TagSystem _tag = default!;

    protected override void Effect(Entity<TagComponent> ent, ref EntityEffectEvent<AddTag> args)
    {
        _tag.AddTag(ent, args.Effect.Tag);
    }
}
