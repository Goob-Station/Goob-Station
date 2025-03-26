using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Goobstation.Shared.MisandryBox.Smites;

public sealed class FlipEyeSystem : ToggleableSmiteSystem<FlipEyeComponent>
{
    [Dependency] private readonly SharedContentEyeSystem _eyeSystem = default!;

    public override void Set(EntityUid owner)
    {
        EnsureComp<ContentEyeComponent>(owner, out var comp);
        _eyeSystem.SetZoom(owner, comp.TargetZoom * -1, ignoreLimits: true);
    }
}
