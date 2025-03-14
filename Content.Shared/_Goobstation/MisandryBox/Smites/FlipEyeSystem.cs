using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared._Goobstation.MisandryBox.Smites;

public sealed class FlipEyeSystem : ToggleableSmiteSystem<FlipEyeComponent>
{
    [Dependency] private readonly SharedContentEyeSystem _eyeSystem = default!;


    public override void Set(EntityUid owner)
    {
        EnsureComp<ContentEyeComponent>(owner, out var comp);
        _eyeSystem.SetZoom(owner, comp.TargetZoom * -1, ignoreLimits: true);
    }
}
