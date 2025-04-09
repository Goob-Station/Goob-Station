using Content.Goobstation.Server.MimePunishment;
using Content.Goobstation.Shared.MimePunishment;
using Content.Shared.Abilities.Mime;
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
        SubscribeLocalEvent<MimeryPowersComponent, BreakVowAlertEvent>(OnBreakVowAlert);
    }

    private void OnInit(Entity<MimeryPowersComponent> ent, ref ComponentInit args)
    {
        _popupSystem.PopupEntity(Loc.GetString("mimery-powers-obtained"), ent, ent);
        EnsureComp<FingerGunComponent>(ent);
        _actions.AddAction(ent, ref ent.Comp.MimeryWallPower, "ActionMimeryWall", ent);
    }

    private void OnBreakVowAlert(Entity<MimeryPowersComponent> ent, ref BreakVowAlertEvent args)
    {
        RaiseLocalEvent(ent, new MimePunishEvent(1));
        _actions.RemoveAction(ent, ent.Comp.MimeryWallPower);
    }
}
