using Content.Server.Actions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cult.Magic;
public sealed partial class BloodMagicSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodMagicProviderComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(Entity<BloodMagicProviderComponent> ent, ref ComponentInit args)
    {
        if (!_actions.TryGetActionById(ent, ent.Comp.SpellsProviderActionId, out _))
            _actions.AddAction(ent, ent.Comp.SpellsProviderActionId);
    }

    public bool TryGrantSpell(Entity<BloodMagicProviderComponent> ent, EntProtoId spellId)
    {
        _actions.AddAction(ent, spellId);
        return true;
    }
}
