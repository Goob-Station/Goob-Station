using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.Borgs;

public abstract class SharedBorgSwitchableSubtypeSystem : EntitySystem
{
    // TODO: remove bug with BorgVisualLayers & fix midround borgs type switch
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, BorgSelectSubtypeMessage>(OnSubtypeSelection);
    }

    private void OnComponentInit(Entity<BorgSwitchableSubtypeComponent> ent, ref ComponentInit args)
    {
        if(ent.Comp.BorgSubtype == null)
            return;

        SetAppearanceFromSubtype(ent, ent.Comp.BorgSubtype.Value);
    }
    private void OnSubtypeSelection(Entity<BorgSwitchableSubtypeComponent> ent, ref BorgSelectSubtypeMessage args)
    {
        SetSubtype(ent, args.Subtype);
    }

    protected virtual void SetAppearanceFromSubtype(Entity<BorgSwitchableSubtypeComponent> ent, ProtoId<BorgSubtypePrototype> subtype)
    {
    }

    public void SetSubtype(Entity<BorgSwitchableSubtypeComponent> ent, ProtoId<BorgSubtypePrototype> subtype)
    {
        ent.Comp.BorgSubtype = subtype;
        var ev = new BorgSubtypeChangedEvent(subtype);
        RaiseLocalEvent(ent, ref ev);
    }
}
