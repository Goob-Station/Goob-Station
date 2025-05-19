// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Bandit <queenjess521@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.TheManWhoSoldTheWorld;
using Content.Goobstation.Common.Weapons.Multishot;
using Content.Goobstation.Common.Weapons.NoWieldNeeded;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Smoking;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
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

    private const string LitPrefix = "lit";
    private const string UnlitPrefix = "unlit";
    private const string MaskSlot = "mask";

    /// <inheritdoc/>o
    public override void Initialize()
    {
        SubscribeLocalEvent<HoloCigarComponent, GetVerbsEvent<AlternativeVerb>>(OnAddInteractVerb);
        SubscribeLocalEvent<HoloCigarComponent, ComponentHandleState>(OnComponentHandleState);

        SubscribeLocalEvent<HoloCigarAffectedGunComponent, DroppedEvent>(OnDroppedEvent);

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

        _audio.Stop(holoCigarComponent.MusicEntity);
        ShutDownEnumerateRemoval(ent);

        if (!ent.Comp.AddedNoWieldNeeded)
            return;

        RemComp<NoWieldNeededComponent>(ent);
        ent.Comp.HoloCigarEntity = null;
    }

    private void ShutDownEnumerateRemoval(Entity<TheManWhoSoldTheWorldComponent> ent)
    {
        var query = EntityQueryEnumerator<HoloCigarAffectedGunComponent>();
        while (query.MoveNext(out var gun, out var comp))
        {
            if (comp.GunOwner != ent.Owner)
                continue;

            RestoreGun(gun);
        }
    }

    private void OnMapInitEvent(Entity<TheManWhoSoldTheWorldComponent> ent, ref MapInitEvent args)
    {
        if (!HasComp<NoWieldNeededComponent>(ent))
        {
            ent.Comp.AddedNoWieldNeeded = true;
            AddComp<NoWieldNeededComponent>(ent);
        }
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
        affected.GunOwner = ent.Owner;

        if (TryComp<MultishotComponent>(args.Item, out var multi))
        {
            affected.WasOriginallyMultishot = true;
            affected.OriginalMissChance = multi.MissChance;
            affected.OriginalSpreadModifier = multi.SpreadMultiplier;
            affected.OriginalSpreadAddition = multi.SpreadAddition;
            affected.OriginalHandDamageAmount = multi.HandDamageAmount; // We don't care about the type though
            affected.OriginalStaminaDamage = multi.StaminaDamage;
        }

        multi = EnsureComp<MultishotComponent>(args.Item);
        multi.MissChance = 0f;
        multi.SpreadMultiplier = 1f; // no extra spread chuds
        multi.SpreadAddition = 0f;
        multi.HandDamageAmount = 0f;
        multi.StaminaDamage = 0f;

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
            var audio = _audio.PlayPvs(ent.Comp.Music, ent);

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
                multiShotComp.MissChance = cigarAffectedGunComponent.OriginalMissChance;
                multiShotComp.SpreadMultiplier = cigarAffectedGunComponent.OriginalSpreadModifier;
                multiShotComp.SpreadAddition = cigarAffectedGunComponent.OriginalSpreadAddition;
                multiShotComp.HandDamageAmount = cigarAffectedGunComponent.OriginalHandDamageAmount;
                multiShotComp.StaminaDamage = cigarAffectedGunComponent.OriginalStaminaDamage;
                break;
            }
        }

        RemComp<HoloCigarAffectedGunComponent>(gun);
        _gun.RefreshModifiers(gun);
    }

    #endregion
}
