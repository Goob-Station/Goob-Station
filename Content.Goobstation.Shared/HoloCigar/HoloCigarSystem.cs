using Content.Goobstation.Shared.Weapons.Multishot;
using Content.Goobstation.Shared.Weapons.RequiresDualWield;
using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Nutrition.Components;
using Content.Shared.Smoking;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.HoloCigar;

/// <summary>
/// This is the system for the Holo-Cigar. - pure unadulterated shitcode below beware
/// </summary>
public sealed class HoloCigarSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedItemSystem _items = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    private const string YowieProtoId = "Yowie";
    /// <inheritdoc/>o
    public override void Initialize()
    {
        SubscribeLocalEvent<HoloCigarComponent, GetVerbsEvent<InteractionVerb>>(OnAddInteractVerb);
        SubscribeLocalEvent<HoloCigarComponent, ComponentHandleState>(OnComponentHandleState);
        SubscribeLocalEvent<HoloCigarAffectedGunComponent, DroppedEvent>(OnDroppedEvent);
        SubscribeLocalEvent<TheManWhoSoldTheWorldComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<TheManWhoSoldTheWorldComponent, MapInitEvent>(OnMapInitEvent);
        SubscribeLocalEvent<TheManWhoSoldTheWorldComponent, ComponentShutdown>(OnComponentShutdown);
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

    private void OnComponentShutdown(Entity<TheManWhoSoldTheWorldComponent> ent, ref ComponentShutdown args)
    {
        ShutDownEnumerateRemoval(ent);

        if (!TryComp<HumanoidAppearanceComponent>(ent, out var appearance) || appearance.Species == YowieProtoId)
            return;

        RemComp<NoWieldNeededComponent>(ent);
    }

    private void ShutDownEnumerateRemoval(Entity<TheManWhoSoldTheWorldComponent> ent)
    {
        var query = EntityQueryEnumerator<HoloCigarAffectedGunComponent, MultishotComponent>();
        while (query.MoveNext(out var gun, out var comp, out var multiShotComp)) // i hate enumerators so much its so ugly can we extend them to have a .ToList() fucking hell
        {
            if (comp.GunOwner != ent.Owner)
                continue;

            switch (comp.WasOriginallyMultishot)
            {
                case false:
                    RemComp<MultishotComponent>(gun);
                    break;
                case true:
                {
                    multiShotComp.SpreadMultiplier = comp.OriginalSpreadModifier;
                    break;
                }
            }// shit code galoreski

            if (comp.GunRequieredWield)
                EnsureComp<GunRequiresWieldComponent>(gun);

            RemComp<HoloCigarAffectedGunComponent>(gun);
            _gun.RefreshModifiers(gun);
        }
    }

    private void OnMapInitEvent(Entity<TheManWhoSoldTheWorldComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<NoWieldNeededComponent>(ent);
    }

    private void OnDroppedEvent(Entity<HoloCigarAffectedGunComponent> ent, ref DroppedEvent args)
    {
        if (!TryComp<MultishotComponent>(ent, out var multishotComponent))
            return;

        switch (ent.Comp.WasOriginallyMultishot)
        {
            case false:
                RemComp<MultishotComponent>(ent);
                break;
            case true:
            {
                multishotComponent.SpreadMultiplier = ent.Comp.OriginalSpreadModifier;
                break;
            }
        }

        if (ent.Comp.GunWasWieldable)
            EnsureComp<WieldableComponent>(ent);
        if (ent.Comp.GunRequieredWield)
            EnsureComp<GunRequiresWieldComponent>(ent);

        RemComp<HoloCigarAffectedGunComponent>(ent);
        _gun.RefreshModifiers(ent.Owner);
    }

    private void OnPickupAttempt(Entity<TheManWhoSoldTheWorldComponent> ent, ref PickupAttemptEvent args)
    {
        if (!HasComp<GunComponent>(args.Item) || HasComp<HoloCigarAffectedGunComponent>(args.Item))
            return;

        var affected = EnsureComp<HoloCigarAffectedGunComponent>(args.Item);

        if (HasComp<MultishotComponent>(args.Item))
            affected.WasOriginallyMultishot = true;

        var multi = EnsureComp<MultishotComponent>(args.Item);

        affected.GunOwner = ent.Owner;

        if (HasComp<GunRequiresWieldComponent>(args.Item))
            affected.GunRequieredWield = true;
        if (HasComp<WieldableComponent>(args.Item))
            affected.GunWasWieldable = true;

        RemComp<GunRequiresWieldComponent>(args.Item);
        RemComp<WieldableComponent>(args.Item);
        affected.OriginalSpreadModifier = multi.SpreadMultiplier;
        multi.SpreadMultiplier = 1f; // no extra spread chuds
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

        if(!_net.IsServer) // mary copium right here
            return;

        if (ent.Comp.Lit == false)
        {
            var audio = _audio.PlayPvs(ent.Comp.Music, ent, AudioParams.Default.WithLoop(true).WithVolume(2f));
            if (audio is null)
                return;
            ent.Comp.MusicEntity = audio.Value.Entity;
            return;
        }

        _audio.Stop(ent.Comp.MusicEntity);
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
