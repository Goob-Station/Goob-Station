using Content.Shared.DoAfter;
using Content.Shared.Store;
using Content.Shared.Popups;
using Content.Shared.Store.Components;
using Content.Shared.Interaction;
using Content.Server.Store.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Content.Goobstation.Shared.NTR.Scan;
using Content.Goobstation.Common.NTR.Scan;

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
        }

        private void OnAfterInteract(EntityUid uid, BriefcaseScannerComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach || args.Target == null)
                return;
            if (TryComp<StoreComponent>(uid, out var store) && store.OwnerOnly)
            {
                if (!_mind.TryGetMind(args.User, out var mindId, out _) || store.AccountOwner != mindId)
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
            if (TryComp<StoreComponent>(uid, out var store) && store.CurrencyWhitelist.Contains("NTLoyaltyPoint"))
            {
                var points = scannable.Points;
                if (points <= 0)
                {
                    _popup.PopupEntity("womp womp", uid, args.User);
                }
                else
                {
                    _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>
                    {
                        { "NTLoyaltyPoint", FixedPoint2.New(points) }
                    }, uid, store);
                }
            }

            args.Handled = true;
        }
    }
}
