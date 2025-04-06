using Content.Shared.Actions;
using Robust.Shared.Player;

namespace Content.Shared._Shitcode.Mimery;

public sealed class FingerGunSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FingerGunComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FingerGunComponent, FingerGunEvent>(OnUse);
    }

    private void OnInit(Entity<FingerGunComponent> ent, ref ComponentInit args)
    {
        string test = ent.ToString();
        Logger.Debug(test);
        _actions.AddAction(ent, "ActionFingerGun"); // NO WORKY D:
    }

    private void OnUse(Entity<FingerGunComponent> ent, ref FingerGunEvent args)
    {

    }
}
