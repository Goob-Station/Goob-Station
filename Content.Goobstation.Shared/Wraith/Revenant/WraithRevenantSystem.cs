using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Revenant;

/// <summary>
/// This handles the revenant system for wraith.
/// Just adds the abilities
/// </summary>
public sealed class WraithRevenantSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithRevenantComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<WraithRevenantComponent> ent, ref MapInitEvent args) =>
        EntityManager.AddComponents(ent.Owner, _proto.Index(ent.Comp.RevenantAbilities));
}
