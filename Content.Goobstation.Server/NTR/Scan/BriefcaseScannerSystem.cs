using Content.Shared.DoAfter;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Interaction;
using Content.Server.Store.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Content.Shared.Mind;
using Robust.Shared.Player;
using Content.Shared.UserInterface;
using Content.Goobstation.Shared.NTR;

namespace Content.Goobstation.Server.NTR.Scan
{
    public sealed class BriefcaseScannerSystem : EntitySystem
    {
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly StoreSystem _storeSystem = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BriefcaseScannerComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<BriefcaseScannerComponent, BriefcaseScannerDoAfterEvent>(OnDoAfter);
            SubscribeLocalEvent<StoreComponent, ActivatableUIOpenAttemptEvent>(OnStoreOpenAttempt);
        }

        private void OnStoreOpenAttempt(EntityUid uid, StoreComponent component, ActivatableUIOpenAttemptEvent args)
        { // copy pasted
            if (!component.OwnerOnly)
                return;
            if (!_mind.TryGetMind(args.User, out var mindId, out var mind))
                return;
            component.AccountOwner ??= mind;
            DebugTools.Assert(component.AccountOwner != null);

            if (component.AccountOwner == mind)
                return;
            _popup.PopupEntity(Loc.GetString("store-not-account-owner", ("store", uid)), uid, args.User);
            args.Cancel();
        }
        private void OnAfterInteract(EntityUid uid, BriefcaseScannerComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach || args.Target == null)
                return;
            // cant open the store if ur not ntr
            if (TryComp<StoreComponent>(uid, out var store) && store.OwnerOnly)
            {
                if (!_mind.TryGetMind(args.User, out var mindId, out var mind) || store.AccountOwner != mind)
                {
                    _popup.PopupEntity(Loc.GetString("store-not-account-owner", ("store", uid)), uid, args.User);
                    return;
                }
            }
            var target = args.Target.Value;

            if (!TryComp<ScannableForPointsComponent>(target, out var scannable) || scannable.AlreadyScanned)
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.ScanDuration,
                new BriefcaseScannerDoAfterEvent(), uid, target: target, used: uid)
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
            if (args.Cancelled || args.Handled || args.Target == null)
                return;
            var target = args.Target.Value;
            if (!TryComp<ScannableForPointsComponent>(target, out var scannable) || scannable.AlreadyScanned)
                return;
            scannable.AlreadyScanned = true;
            //Dirty(target, scannable);
            if (TryComp<StoreComponent>(uid, out var store))
            {
                var currency = new Dictionary<string, FixedPoint2>
                {
                    { "NTLoyaltyPoint", FixedPoint2.New(scannable.Points) }
                };

                if (store.CurrencyWhitelist.Contains("NTLoyaltyPoint"))
                {
                    _storeSystem.TryAddCurrency(currency, uid, store);
                    DebugTools.Assert(store.Balance.ContainsKey("NTLoyaltyPoint"));
                }
            }

            args.Handled = true;
        }
    }
}
