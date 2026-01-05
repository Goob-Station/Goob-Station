// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.NTR.Scan;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.NTR.Scan;
using Content.Server.Chat.Systems;
using Content.Server.Store.Systems;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Server.NTR.Scan
{
    public sealed class BriefcaseScannerSystem : EntitySystem
    {
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly StoreSystem _storeSystem = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly ChatSystem _chatManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BriefcaseScannerComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<BriefcaseScannerComponent, BriefcaseScannerDoAfterEvent>(OnDoAfter);
        }

        private void OnAfterInteract(EntityUid uid, BriefcaseScannerComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach
                || args.Target == null)
                return;

            if (TryComp<StoreComponent>(uid, out var store)
                && store.OwnerOnly)
            {
                if (!_mind.TryGetMind(args.User, out var mindId, out _)
                    || store.AccountOwner != mindId)
                    _popup.PopupEntity(Loc.GetString("store-not-account-owner", ("store", uid)), uid, args.User);
                return;
            }
            var target = args.Target.Value;

            if (!TryComp<ScannableForPointsComponent>(target, out var scannable)
                || scannable.AlreadyScanned)
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager,
                args.User,
                component.ScanDuration,
                new BriefcaseScannerDoAfterEvent(),
                uid,
                target: target,
                used: uid)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                NeedHand = true,
                BreakOnHandChange = true,
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }

        private void OnDoAfter(EntityUid uid, BriefcaseScannerComponent component, BriefcaseScannerDoAfterEvent args)
        {
            if (args.Cancelled
                || args.Handled
                || args.Target == null)
                return;

            var target = args.Target.Value;

            if (!TryComp<ScannableForPointsComponent>(target, out var scannable)
                || scannable.AlreadyScanned)
                return;

            scannable.AlreadyScanned = true;
            //Dirty(target, scannable);

            if (TryComp<StoreComponent>(uid, out var store) && store.CurrencyWhitelist.Contains("NTLoyaltyPoint"))
            {
                var points = scannable.Points;
                if (points <= 0)
                    _chatManager.TrySendInGameICMessage(uid, Loc.GetString("ntr-scan-fail"), InGameICChatType.Speak, true);

                else
                {
                    _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2> {
                        { "NTLoyaltyPoint", FixedPoint2.New(points) } },
                    uid,
                    store);
                    _chatManager.TrySendInGameICMessage(uid, Loc.GetString("ntr-scan-success", ("amount", points)), InGameICChatType.Speak, true);

                    QueueDel(target);
                    Spawn("BluespaceTeleportationEffect", Transform(target).Coordinates);
                }
            }

            args.Handled = true;
        }
    }
}
