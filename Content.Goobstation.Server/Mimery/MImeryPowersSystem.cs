using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Mimery;

public sealed class MimeryPowersSystem : EntitySystem
{

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<MimeryPowersComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<MimeryPowersComponent> ent, ref ComponentInit args)
    {
        _popupSystem.PopupEntity(Loc.GetString("mimery-powers-obtained"), ent, ent);
        EnsureComp<FingerGunComponent>(ent);
        _actions.AddAction(ent, "ActionMimeryWall");
    }
}
