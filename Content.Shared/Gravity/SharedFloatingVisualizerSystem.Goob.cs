namespace Content.Shared.Gravity;

public abstract partial class SharedFloatingVisualizerSystem
{
    protected bool ShouldFloatRegardlessOfGravity(Entity<FloatingVisualsComponent> entity)
    {
        return entity.Comp.FloatRegardlessOfGravity;
    }
}
