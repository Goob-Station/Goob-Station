using Content.Shared.Actions;

namespace Content.Server._Shitcode.Mimery;

public sealed class MimeryPowersSystem : EntitySystem
{

    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MimeryPowersComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<MimeryPowersComponent> ent, ref ComponentInit args)
    {
        EnsureComp<FingerGunComponent>(ent);
        _actions.AddAction(ent, "ActionMimeryWall");
    }
}
