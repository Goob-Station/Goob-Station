using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Body.Components;
using Content.Goobstation.Common.Temperature.Components;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Goobstation.Shared.Voidwalker.GlassPasser;
using Content.Goobstation.Shared.Voidwalker.Voided;
using Content.Shared.Charges.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Voidwalker.CosmicSkull;

public sealed partial class CosmicSkullSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedChargesSystem _charges = null!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicSkullComponent, UseInHandEvent>(OnCosmicSkullUsed);
        SubscribeLocalEvent<CosmicSkullComponent, CosmicSkullDoAfterEvent>(OnCosmicSkullDoAfter);
    }

    private void OnCosmicSkullUsed(Entity<CosmicSkullComponent> skull, ref UseInHandEvent args)
    {
        if (args.Handled
            || !_charges.HasCharges(skull.Owner, 1))
            return;


        if (HasComp<VoidwalkerComponent>(args.User))
        {
            var voidwalkerFailPopup = Loc.GetString("cosmic-skull-use-voidwalker");
            _popupSystem.PopupClient(voidwalkerFailPopup, args.User, args.User, PopupType.LargeCaution);

            return;
        }

        var startPopup = Loc.GetString("cosmic-skull-use-start", ("object", Name(skull)));
        _popupSystem.PopupClient(startPopup, args.User, args.User);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            skull.Comp.DoAfterDuration,
            new CosmicSkullDoAfterEvent(),
            skull)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnCosmicSkullDoAfter(Entity<CosmicSkullComponent> skull, ref CosmicSkullDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        RemComp<VoidedComponent>(args.User);
        EnsureComp<SpecialPressureImmunityComponent>(args.User);
        EnsureComp<SpecialBreathingImmunityComponent>(args.User);
        EnsureComp<SpecialLowTempImmunityComponent>(args.User);
        EnsureComp<SpecialHighTempImmunityComponent>(args.User);
        EnsureComp<VoidedVisualsComponent>(args.User);
        EnsureComp<GlassPasserComponent>(args.User);

        _damageable.SetDamageModifierSetId(args.User, skull.Comp.GlassModifierSet);

        var popup = Loc.GetString("cosmic-skull-use-finish");
        _popupSystem.PopupClient(popup, args.User, args.User);

        _charges.TryUseCharge(skull.Owner);
    }
}
