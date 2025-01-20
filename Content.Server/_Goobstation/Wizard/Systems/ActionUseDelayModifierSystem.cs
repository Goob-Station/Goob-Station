using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Actions;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class ActionUseDelayModifierSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionUseDelayModifierComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ActionUseDelayModifierComponent> ent, ref MapInitEvent args)
    {
        _actions.SetUseDelay(ent.Owner, ent.Comp.UseDelay);
    }
}
