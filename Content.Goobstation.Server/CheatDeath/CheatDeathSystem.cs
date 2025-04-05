using Content.Goobstation.Shared.CheatDeath;
using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.CheatDeath;

public sealed partial class CheatDeathSystem : EntitySystem
{
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CheatDeathComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CheatDeathComponent, CheatDeathEvent>(OnDeathCheatAttempt);
        SubscribeLocalEvent<CheatDeathComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStartup(EntityUid uid, CheatDeathComponent comp, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, "ActionCheatDeath");
    }

    private void OnExamined(Entity<CheatDeathComponent> comp, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient && args.Examined != args.Examiner && comp.Comp.ReviveAmount > 0)
            args.PushMarkup(Loc.GetString("cheat-death-component-examined", ("target", Identity.Entity(comp, EntityManager))));

        if (args.Examined == args.Examiner && !_net.IsClient)
            args.PushMarkup(Loc.GetString("cheat-death-component-remaining-revives", ("amount", comp.Comp.ReviveAmount)));
    }

    private void OnDeathCheatAttempt(EntityUid uid, CheatDeathComponent comp, CheatDeathEvent args)
    {
        if (args.Handled || !_mobStateSystem.IsDead(uid))
            return;

        // If the entity is not dead, or if the entity is out of revives, return.
        if (comp.ReviveAmount == 0)
        {
            var failPopup = Loc.GetString("action-cheat-death-fail");
            _popupSystem.PopupEntity(failPopup, uid, PopupType.LargeCaution);

            return;
        }

        // Revive entity
        _rejuvenateSystem.PerformRejuvenate(uid);

        // Show popup
        var popup = Loc.GetString("action-cheated-death");
        _popupSystem.PopupEntity(popup, uid, PopupType.LargeCaution);

        // Decrement remaining revives.
        if (comp.ReviveAmount != -1)
            comp.ReviveAmount--;
        args.Handled = true;

    }
}
