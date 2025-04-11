using Content.Goobstation.Shared.Weapons.Multishot;
using Content.Goobstation.Shared.Weapons.RequiresDualWield;
using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Nutrition.Components;
using Content.Shared.Smoking;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.HoloCigar;

/// <summary>
/// This is the system for the Holo-Cigar.
/// </summary>
public sealed class HoloCigarSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedItemSystem _items = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<HoloCigarComponent, GetVerbsEvent<InteractionVerb>>(OnAddInteractVerb);
        SubscribeLocalEvent<HoloCigarComponent, ComponentHandleState>(OnComponentHandleState);
        SubscribeLocalEvent<HoloCigarUserComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<HoloCigarUserComponent, UnequippedHandEvent >(OnDroppedEvent);
        SubscribeLocalEvent<HoloCigarUserComponent, MapInitEvent>(OnMapInitEvent);
        SubscribeLocalEvent<HoloCigarUserComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnAddInteractVerb(Entity<HoloCigarComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands is null)
            return;

        InteractionVerb verb = new()
        {
            Act = () =>
            {
                HandleToggle(ent);

                ent.Comp.Lit = !ent.Comp.Lit;
                Dirty(ent);
            },
            Message = Loc.GetString("action-description-internals-toggle"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/clock.svg.192dpi.png")),
            Text = Loc.GetString("solution-container-mixer-activate"), // dont ask bruh
        };

        args.Verbs.Add(verb);
    }

    private void OnComponentShutdown(Entity<HoloCigarUserComponent> ent, ref ComponentShutdown args)
    {
        RemComp<NoWieldNeededComponent>(ent);
    }

    private void OnMapInitEvent(Entity<HoloCigarUserComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<NoWieldNeededComponent>(ent);
    }

    private void OnDroppedEvent(Entity<HoloCigarUserComponent> ent, ref UnequippedHandEvent  args)
    {
        RemComp<MultishotComponent>(args.Unequipped);
        _gun.RefreshModifiers(args.Unequipped);
    }

    private void OnPickupAttempt(Entity<HoloCigarUserComponent> ent, ref PickupAttemptEvent args)
    {
        if (!HasComp<GunComponent>(args.Item))
            return;

        EnsureComp<MultishotComponent>(args.Item);
        _gun.RefreshModifiers(args.Item);
    }

    private void HandleToggle(Entity<HoloCigarComponent> ent, AppearanceComponent? appearance = null, ClothingComponent? clothing = null)
    {
        if (!Resolve(ent, ref appearance, ref clothing) || !_gameTiming.IsFirstTimePredicted) // fuck predicting this shit
            return;

        var state = ent.Comp.Lit ? SmokableState.Unlit : SmokableState.Lit;
        var prefix = ent.Comp.Lit ? "unlit" : "lit";

        _appearance.SetData(ent, SmokingVisuals.Smoking, state, appearance);
        _clothing.SetEquippedPrefix(ent, prefix, clothing);
        _items.SetHeldPrefix(ent, prefix);
    }
    private void OnComponentHandleState(Entity<HoloCigarComponent> ent, ref ComponentHandleState args)
    {
        if (args.Current is not HoloCigarComponentState state)
            return;

        if (ent.Comp.Lit == state.Lit)
            return;

        ent.Comp.Lit = state.Lit;
        HandleToggle(ent);
    }
}
