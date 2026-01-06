// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Shared._Lavaland.Audio;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Body.Systems;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract partial class SharedCyberdeckSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;
    [Dependency] protected readonly SharedChargesSystem Charges = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedBossMusicSystem _bossMusic = default!;
    [Dependency] private readonly SharedCryostorageSystem _cryostorage = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly INetManager _net = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<ContainerManagerComponent> _containerQuery;
    private EntityQuery<LimitedChargesComponent> _chargesQuery;
    private EntityQuery<CyberdeckHackableComponent> _hackableQuery;
    private EntityQuery<CyberdeckUserComponent> _cyberdeckUserQuery;
    private EntityQuery<StationAiWhitelistComponent> _aiWhitelistQuery;

    public override void Initialize()
    {
        base.Initialize();

        InitializeUser();
        InitializeCharges();
        InitializeHacking();
        InitializeProjection();

        _handsQuery = GetEntityQuery<HandsComponent>();
        _containerQuery = GetEntityQuery<ContainerManagerComponent>();
        _chargesQuery = GetEntityQuery<LimitedChargesComponent>();
        _hackableQuery = GetEntityQuery<CyberdeckHackableComponent>();
        _cyberdeckUserQuery = GetEntityQuery<CyberdeckUserComponent>();
        _aiWhitelistQuery = GetEntityQuery<StationAiWhitelistComponent>();
    }

    /// <summary>
    /// Updates an alert, counting how many charges player currently has.
    /// </summary>
    /// <param name="ent">A user to apply the alert.</param>
    /// <param name="doClear">If true, will just remove the alert entirely, until it gets updated again.</param>
    private void UpdateAlert(Entity<CyberdeckUserComponent> ent, bool doClear = false)
    {
        if (doClear || ent.Comp.ProviderEntity == null)
        {
            _alerts.ClearAlert(ent.Owner, ent.Comp.AlertId);
            return;
        }

        var charges = Charges.GetCurrentCharges(ent.Comp.ProviderEntity.Value);
        var severity = (short) Math.Clamp(charges, 0, 8);
        _alerts.ShowAlert(ent.Owner, ent.Comp.AlertId, severity);
    }
}
