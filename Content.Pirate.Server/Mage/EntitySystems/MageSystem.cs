using Content.Pirate.Server.Mage.Components;
using Content.Server.Actions;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Pirate.Shared.Mage;
using Content.Pirate.Shared.Mage.Components;
using Content.Shared.Examine;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageSystem : EntitySystem
{
    [ValidatePrototypeId<EntityPrototype>]
    private const string MageShopId = "ActionMageShop";

    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MageManaSystem _power = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MageComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<MageComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MageComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<MageComponent, MageShopActionEvent>(OnShop);
        SubscribeLocalEvent<MageComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<MageComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, MageComponent component, ComponentStartup args)
    {
        if (TryComp<StoreComponent>(uid, out var store))
            _store.UpdateUserInterface(uid, uid, store);

        _store.TryAddCurrency(new Dictionary<string, FixedPoint2>
            { { component.ExperinceCurrencyPrototype, 10 } }, uid);
    }

    private void OnExamine(EntityUid uid, MageComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var powerType = _power.GetLevelName(component.ManaLevel);

        // Show exact values for yourself
        if (args.Examined == args.Examiner)
        {
            args.PushMarkup(Loc.GetString("shadowkin-power-examined-self",
                ("power", (int) component.ManaLevel),
                ("powerMax", component.ManaLevelMax),
                ("powerType", powerType)
            ));
        }
        // Show general values for others
        else
        {
            args.PushMarkup(Loc.GetString("shadowkin-power-examined-other",
                ("target", Identity.Entity(uid, _entity)),
                ("powerType", powerType)
            ));
        }
    }

    private void OnInit(EntityUid uid, MageComponent component, ComponentInit args)
    {
        if (component.ManaLevel <= MageComponent.ManaThresholds[ManaThreshold.Min] + 1f)
            _power.SetPowerLevel(uid, MageComponent.ManaThresholds[ManaThreshold.Good]);

        _power.UpdateAlert(uid, true, component.ManaLevel);
    }

    private void OnShutdown(EntityUid uid, MageComponent component, ComponentShutdown args)
    {
        _power.UpdateAlert(uid, false);
    }

    private void OnShop(EntityUid uid, MageComponent component, MageShopActionEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;
        _store.ToggleUi(uid, uid, store);

        if (!TryComp<ExperinceComponent>(uid, out var experince))
            return;

        // _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { TelecrystalCurrencyPrototype, balance.Value } }, uplinkEntity.Value, store);

        // _store.TryAddCurrency(new Dictionary<string, FixedPoint2>
        //     { {component.ExperinceCurrencyPrototype, experince.ExperinceAmount} }, uid);
    }

    private void OnMapInit(EntityUid uid, MageComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.Action, MageShopId);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = _entity.EntityQueryEnumerator<MageComponent>();

        // Update power level for all shadowkin
        while (query.MoveNext(out var uid, out var mage))
        {
            // I can't figure out how to get this to go to the 100% filled state in the above if statement 😢
            _power.UpdateAlert(uid, true, mage.ManaLevel);
            _power.TryUpdatePowerLevel(uid, frameTime);
        }
    }
}
