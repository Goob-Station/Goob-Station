using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Body.Components;
using Content.Goobstation.Common.Temperature.Components;
using Content.Goobstation.Common.VoidedVisualizer;
using Content.Goobstation.Shared.Voidwalker;
using Content.Goobstation.Shared.Voidwalker.CosmicSkull;
using Content.Goobstation.Shared.Voidwalker.GlassPasser;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Voidwalker.CosmicSkull;

public sealed partial class CosmicSkullSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;

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
            || skull.Comp.Uses <= 0)
            return;

        if (HasComp<VoidwalkerComponent>(args.User))
        {
            var voidwalkerFailPopup = Loc.GetString("cosmic-skull-use-voidwalker");
            _popupSystem.PopupEntity(voidwalkerFailPopup, args.User, args.User, PopupType.LargeCaution);

            return;
        }

        var startPopup = Loc.GetString("cosmic-skull-use-start", ("object", Name(skull)));
        _popupSystem.PopupEntity(startPopup, args.User, args.User);

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
            || args.Cancelled
            || skull.Comp.Uses <= 0)
            return;

        skull.Comp.Uses--;

        if (skull.Comp.Uses <= 0)
            QueueDel(skull);

        EnsureComp<SpecialPressureImmunityComponent>(args.User);
        EnsureComp<SpecialBreathingImmunityComponent>(args.User);
        EnsureComp<SpecialLowTempImmunityComponent>(args.User);
        EnsureComp<SpecialHighTempImmunityComponent>(args.User);
        EnsureComp<VoidedVisualsComponent>(args.User);
        EnsureComp<GlassPasserComponent>(args.User);

        _damageable.SetDamageModifierSetId(args.User, skull.Comp.GlassModifierSet);

        var popup = Loc.GetString("cosmic-skull-use-finish");
        _popupSystem.PopupEntity(popup, args.User, args.User);
    }
}
