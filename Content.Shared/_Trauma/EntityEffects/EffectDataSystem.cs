namespace Content.Shared._Trauma.EntityEffects;

/// <summary>
/// API for passing extra data to effects via storing a temporary component on the target entity.
/// When setting data make sure to remove it after the effect has ran via passing null.
/// </summary>
/// <remarks>
/// The only reason this exists is because "ECS" entity effects are half baked and provide no way to extend the arguments like the original did.
/// TheShuEd my goat.
/// </remarks>
public sealed partial class EffectDataSystem : EntitySystem
{
    private EntityQuery<EntityEffectToolComponent> _toolQuery;

    public override void Initialize()
    {
        base.Initialize();

        _toolQuery = GetEntityQuery<EntityEffectToolComponent>();
    }

    public EntityUid? GetTool(EntityUid target)
        => _toolQuery.CompOrNull(target)?.Tool;

    public void SetTool(EntityUid target, EntityUid tool)
    {
        // don't add components to entities being deleted, 0.001% edge case but still
        if (TerminatingOrDeleted(target))
            return;

        EnsureComp<EntityEffectToolComponent>(target).Tool = tool;
    }

    public void ClearTool(EntityUid target)
        => RemComp<EntityEffectToolComponent>(target);

    public void CopyData(EntityUid src, EntityUid target)
    {
        // TODO: if you add any more data change these methods to raise events
        if (GetTool(src) is { } tool)
            SetTool(target, tool);
    }

    public void ClearData(EntityUid target)
    {
        ClearTool(target);
    }
}

/// <summary>
/// Temporary data component that stores the tool used to cause an effect to occur.
/// </summary>
[RegisterComponent, Access(typeof(EffectDataSystem))]
public sealed partial class EntityEffectToolComponent : Component
{
    [DataField]
    public EntityUid Tool;
}
