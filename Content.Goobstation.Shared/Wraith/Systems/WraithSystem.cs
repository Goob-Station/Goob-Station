using Content.Goobstation.Shared.Wraith.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class WraithSystem : EntitySystem
{
    [Dependency] private  readonly IPrototypeManager _proto  = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WraithComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<WraithComponent> ent, ref MapInitEvent args) =>
        EntityManager.AddComponents(ent.Owner, _proto.Index(ent.Comp.Abilities));
}
