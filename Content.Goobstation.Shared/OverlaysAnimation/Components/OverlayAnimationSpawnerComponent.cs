using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

[RegisterComponent]
public sealed partial class OverlayAnimationSpawnerComponent : Component
{
    [DataField(required: true)]
    public EntityTableSelector AnimationsTable = default!;
}
