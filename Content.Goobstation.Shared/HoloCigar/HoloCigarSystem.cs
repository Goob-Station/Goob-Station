// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.TheManWhoSoldTheWorld;
using Content.Goobstation.Shared.Weapons.Multishot;
using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Smoking;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
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
    [Dependency] private readonly InventorySystem _inventory = default!;

    private const string YowieProtoId = "Yowie";
    private const string LitPrefix = "lit";
    private const string UnlitPrefix = "unlit";
    private const string MaskSlot = "mask";

    /// <inheritdoc/>o
    public override void Initialize()
    {
        SubscribeLocalEvent<HoloCigarComponent, GetVerbsEvent<AlternativeVerb>>(OnAddInteractVerb);
        SubscribeLocalEvent<HoloCigarComponent, ComponentHandleState>(OnComponentHandleState);

        SubscribeLocalEvent<HoloCigarAffectedGunComponent, DroppedEvent>(OnDroppedEvent);
        SubscribeLocalEvent<HoloCigarAffectedGunComponent, WieldAttemptEvent>(OnWieldAttemptEvent);

        SubscribeLocalEvent<TheManWhoSoldTheWorldComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<TheManWhoSoldTheWorldComponent, MapInitEvent>(OnMapInitEvent);
        SubscribeLocalEvent<TheManWhoSoldTheWorldComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<TheManWhoSoldTheWorldComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
    }

    private void OnAddInteractVerb(Entity<HoloCigarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands is null)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                HandleToggle(ent);
                ent.Comp.Lit = !ent.Comp.Lit;
                Dirty(ent);
            },
            Message = Loc.GetString("holo-cigar-verb-desc"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/clock.svg.192dpi.png")),
            Text = Loc.GetString("holo-cigar-verb-text"),
        };

        args.Verbs.Add(verb);
    }

    #region Event Methods

    private void OnWieldAttemptEvent(Entity<HoloCigarAffectedGunComponent> ent, ref WieldAttemptEvent args)
    {
        args.Cancel(); // cancel any attempts to wield holocigar weapons
    }

    private void OnMobStateChangedEvent(Entity<TheManWhoSoldTheWorldComponent> ent, ref MobStateChangedEvent args)
    {
        if (!TryComp<HoloCigarComponent>(ent.Comp.HoloCigarEntity, out var holoCigarComponent))
            return;

        if (args.NewMobState == MobState.Dead)
            _audio.Stop(holoCigarComponent.MusicEntity); // no music out of mouth duh

        if (_net.IsServer)
            _audio.PlayPvs(ent.Comp.DeathAudio, ent, AudioParams.Default.WithVolume(3f));
    }

    private void OnComponentShutdown(Entity<TheManWhoSoldTheWorldComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<HoloCigarComponent>(ent.Comp.HoloCigarEntity, out var holoCigarComponent))
            return;

        _audio.Stop(holoCigarComponent.MusicEntity); // no music out of mouth duh
        ShutDownEnumerateRemoval(ent);

        if (!TryComp<HumanoidAppearanceComponent>(ent, out var appearance) || appearance.Species == YowieProtoId)
            return;

        RemComp<NoWieldNeededComponent>(ent);
        ent.Comp.HoloCigarEntity = null;
    }

    private void ShutDownEnumerateRemoval(Entity<TheManWhoSoldTheWorldComponent> ent)
    {
        var query = EntityQueryEnumerator<HoloCigarAffectedGunComponent>();
        while
            (query.MoveNext(out var gun, out var comp))
        {
            if (comp.GunOwner != ent.Owner)
                continue;

            RestoreGun(gun);
        }
    }

    private void OnMapInitEvent(Entity<TheManWhoSoldTheWorldComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<NoWieldNeededComponent>(ent);
        if (!_inventory.TryGetSlotEntity(ent, MaskSlot, out var cigarEntity) ||
            !HasComp<HoloCigarComponent>(cigarEntity))
            return;
        ent.Comp.HoloCigarEntity = cigarEntity;
    }

    private void OnDroppedEvent(Entity<HoloCigarAffectedGunComponent> ent, ref DroppedEvent args)
    {
        RestoreGun(ent);
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
        affected.OriginalSpreadModifier = multi.SpreadMultiplier;
        multi.SpreadMultiplier = 1f; // no extra spread chuds
        _gun.RefreshModifiers(args.Item);
    }

    private void HandleToggle(Entity<HoloCigarComponent> ent,
        AppearanceComponent? appearance = null,
        ClothingComponent? clothing = null)
    {
        if (!Resolve(ent, ref appearance, ref clothing) ||
            !_gameTiming.IsFirstTimePredicted) // fuck predicting this shit
            return;

        var state = ent.Comp.Lit ? SmokableState.Unlit : SmokableState.Lit;
        var prefix = ent.Comp.Lit ? UnlitPrefix : LitPrefix;

        _appearance.SetData(ent, SmokingVisuals.Smoking, state, appearance);
        _clothing.SetEquippedPrefix(ent, prefix, clothing);
        _items.SetHeldPrefix(ent, prefix);

        if (!_net.IsServer) // mary copium right here
            return;

        if (ent.Comp.Lit == false)
        {
            var audio = _audio.PlayPvs(ent.Comp.Music,
                ent,
                AudioParams.Default.WithLoop(true).WithVolume(3f)); // must be louder than everything else on jehovah

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

    #endregion

    #region Helper Methods

    private void RestoreGun(EntityUid gun,
        HoloCigarAffectedGunComponent? cigarAffectedGunComponent = null,
        MultishotComponent? multiShotComp = null)
    {
        if (!Resolve(gun, ref cigarAffectedGunComponent, ref multiShotComp))
            return;

        switch (cigarAffectedGunComponent.WasOriginallyMultishot)
        {
            case false:
                RemComp<MultishotComponent>(gun);
                break;
            case true:
            {
                multiShotComp.SpreadMultiplier = cigarAffectedGunComponent.OriginalSpreadModifier;
                break;
            }
        }

        RemComp<HoloCigarAffectedGunComponent>(gun);
        _gun.RefreshModifiers(gun);
    }

    #endregion
}
