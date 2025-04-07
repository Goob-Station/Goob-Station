using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Weapons.RequiresDualWield;

public sealed class RequiresDualWieldSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RequiresDualWieldComponent, ExaminedEvent>(OnExamineRequires);
        SubscribeLocalEvent<RequiresDualWieldComponent, ShotAttemptedEvent>(OnShootAttempt);
    }

    private void OnShootAttempt(EntityUid uid, RequiresDualWieldComponent component, ref ShotAttemptedEvent args)
    {
        var comp = args.Used.Comp;

        if (!TryComp<HandsComponent>(args.User, out var handsComp))
            return;

        if (handsComp.Count != 2)
            return;

        var EnumeratedItems = _handsSystem.EnumerateHeld(args.User, handsComp);

        if (EnumeratedItems.ToList().Count <= 1)
        {
            args.Cancel();
            DualWieldPopup(component, ref args);
        }

        foreach (var held in EnumeratedItems)
        {
            if (held == uid)
                continue;

            if (HasComp<Multishot.MultishotComponent>(held))
            {
                if (CheckGun(held,component.Whitelist))
                    continue;
            }

            args.Cancel();

            DualWieldPopup(component, ref args);
            break;
        }
    }

    private void OnExamineRequires(Entity<RequiresDualWieldComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.WieldRequiresExamineMessage != null)
            args.PushText(Loc.GetString(entity.Comp.WieldRequiresExamineMessage));
    }

    private void DualWieldPopup(RequiresDualWieldComponent component, ref ShotAttemptedEvent args)
    {
        var time = _timing.CurTime;

        if (time > component.LastPopup + component.PopupCooldown)
        {
            component.LastPopup = time;
            var message = Loc.GetString("dual-wield-component-requires", ("item", args.Used));
            _popupSystem.PopupClient(message, args.Used, args.User);
        }
    }

    private bool CheckGun(EntityUid target, EntityWhitelist? whitelist)
    {
        return _whitelistSystem.IsWhitelistPassOrNull(whitelist, target);
    }
}
