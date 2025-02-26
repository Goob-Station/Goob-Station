using Content.Server.Cargo.Components;
using Content.Shared.Cargo.Components;
using Content.Shared.Clothing;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Verbs;

namespace Content.Shared.Cargo.Systems;

/// <summary>
///     The price gun system! If this component is on an entity, you can scan objects (Click or use verb) to see their price.
/// </summary>
public abstract class SharedPriceGunSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AppraisalHudComponent, ClothingGotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<AppraisalHudComponent, ClothingGotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<PriceGunComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);
        SubscribeLocalEvent<WearingAppraisalHudComponent, GetVerbsEvent<InnateVerb>>(OnInnateVerb); // Reserve-AppraisalHUD
        SubscribeLocalEvent<PriceGunComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnUtilityVerb(EntityUid uid, PriceGunComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Using == null)
            return;

        var verb = new UtilityVerb()
        {
            Act = () =>
            {
                GetPriceOrBounty((uid, component), args.Target, args.User);
            },
            Text = Loc.GetString("price-gun-verb-text"),
            Message = Loc.GetString("price-gun-verb-message", ("object", Identity.Entity(args.Target, EntityManager)))
        };

        args.Verbs.Add(verb);
    }

    // Reserve-AppraisalHUD-Start
    private void OnInnateVerb(EntityUid uid, WearingAppraisalHudComponent component, GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !TryComp<PriceGunComponent>(component.Hud, out var priceGun))
            return;

        var verb = new InnateVerb()
        {
            Act = () =>
            {
                GetPriceOrBounty((component.Hud, priceGun), args.Target, args.User, true);
            },
            Text = Loc.GetString("price-gun-verb-text"),
            Message = Loc.GetString("price-gun-verb-message", ("object", Identity.Entity(args.Target, EntityManager)))
        };

        args.Verbs.Add(verb);
    }

    private void OnEquipped(EntityUid uid, AppraisalHudComponent component, ref ClothingGotEquippedEvent args)
    {
        component.IsActive = true;

        var wearingComp = EnsureComp<WearingAppraisalHudComponent>(args.Wearer);
        wearingComp.Hud = uid;
    }

    private void OnUnequipped(EntityUid uid, AppraisalHudComponent component, ref ClothingGotUnequippedEvent args)
    {
        if (!component.IsActive)
            return;

        RemComp<WearingAppraisalHudComponent>(args.Wearer);
        component.IsActive = false;
    }
    // Reserve-AppraisalHUD-End

    private void OnAfterInteract(Entity<PriceGunComponent> entity, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null || args.Handled)
            return;

        args.Handled |= GetPriceOrBounty(entity, args.Target.Value, args.User);
    }

    /// <summary>
    ///     Find the price or confirm if the item is a bounty. Will give a popup of the result to the passed user.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     This is abstract for prediction. When the bounty system / cargo systems that are necessary are moved to shared,
    ///     combine all the server, client, and shared stuff into one non abstract file.
    /// </remarks>
    protected abstract bool GetPriceOrBounty(Entity<PriceGunComponent> entity, EntityUid target, EntityUid user, bool cooldownPopup = false); // Reserve-CooldownPopup
}
