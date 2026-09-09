// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Weapons.Multihit;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Emp;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MantisBlades;

public sealed class SharedMantisBladeSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MantisBladeArmComponent, MapInitEvent>(OnArmMapInit);
        SubscribeLocalEvent<MantisBladeArmComponent, ComponentRemove>(OnArmRemove);
        SubscribeLocalEvent<MantisBladeArmComponent, BodyPartAddedEvent>(OnArmChanged);
        SubscribeLocalEvent<MantisBladeArmComponent, BodyPartRemovedEvent>(OnArmChanged);
        SubscribeLocalEvent<MantisBladeArmComponent, EmpDisabledRemovedEvent>(OnArmChanged);

        SubscribeLocalEvent<MantisBladeArmComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MantisBladeArmComponent, EmpPulseEvent>(OnEmpPulse);

        SubscribeLocalEvent<MantisBladeUserComponent, MapInitEvent>(OnUserMapInit);
        SubscribeLocalEvent<MantisBladeUserComponent, ComponentShutdown>(OnUserShutdown);
        SubscribeLocalEvent<MantisBladeUserComponent, ToggleMantisBladesActionEvent>(OnToggle);
        SubscribeLocalEvent<MantisBladeUserComponent, GetMeleeWeaponEvent>(OnGetMeleeWeapon);
        SubscribeLocalEvent<MantisBladeUserComponent, MultihitGetWeaponsEvent>(OnGetMultihitWeapons);
    }

    #region Arms

    private void OnExamined(Entity<MantisBladeArmComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("mantis-blade-arm-examine"));
    }

    private void OnArmChanged<T>(Entity<MantisBladeArmComponent> ent, ref T args)
        => RefreshBody(ent);

    private void OnArmMapInit(Entity<MantisBladeArmComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsServer && ent.Comp.Blade == null)
        {
            _container.EnsureContainer<ContainerSlot>(ent, MantisBladeArmComponent.BladeContainer);
            ent.Comp.Blade = SpawnInContainerOrDrop(ent.Comp.BladeProto, ent, MantisBladeArmComponent.BladeContainer);
            Dirty(ent);
        }

        RefreshBody(ent);
    }

    private void OnArmRemove(Entity<MantisBladeArmComponent> ent, ref ComponentRemove args)
    {
        if (_net.IsServer && ent.Comp.Blade is { } blade)
            QueueDel(blade);

        ent.Comp.Blade = null;
        RefreshBody(ent);
    }

    private void OnEmpPulse(Entity<MantisBladeArmComponent> ent, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;

        RefreshBody(ent, disabling: ent);
    }

    private void RefreshBody(EntityUid arm, EntityUid? disabling = null)
    {
        if (TryComp<BodyPartComponent>(arm, out var part) && part.Body is { } body)
            Refresh(body, disabling);
    }

    /// <summary>
    /// adds and removes the <see cref="MantisBladeUserComponent"/>.
    /// </summary>
    private void Refresh(EntityUid body, EntityUid? disabling = null)
    {
        var hasArm = false;
        var blades = new List<EntityUid>();

        foreach (var arm in _body.GetBodyChildrenOfType(body, BodyPartType.Arm))
        {
            if (!TryComp<MantisBladeArmComponent>(arm.Id, out var bladeArm))
                continue;

            hasArm = true;
            if (arm.Id != disabling && !HasComp<EmpDisabledComponent>(arm.Id) && bladeArm.Blade is { } blade)
                blades.Add(blade);
        }

        if (!hasArm)
        {
            RemCompDeferred<MantisBladeUserComponent>(body);
            return;
        }

        var user = EnsureComp<MantisBladeUserComponent>(body);
        user.Blades = blades;
        Dirty(body, user);
    }

    #endregion

    #region MantisBladeUserComp

    private void OnUserMapInit(Entity<MantisBladeUserComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);

        EnsureComp<MultihitComponent>(ent);
    }

    private void OnUserShutdown(Entity<MantisBladeUserComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
        RemCompDeferred<MultihitComponent>(ent);
    }

    private void OnToggle(Entity<MantisBladeUserComponent> ent, ref ToggleMantisBladesActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!ent.Comp.Extended && ent.Comp.Blades.Count == 0)
        {
            _popup.PopupClient(Loc.GetString("mantis-blade-disabled-emp"), ent, ent);
            return;
        }

        ent.Comp.Extended = !ent.Comp.Extended;
        if (ent.Comp.Extended)
            ent.Comp.ExtendedAt = _timing.CurTime;
        Dirty(ent);

        _actions.SetToggled(ent.Comp.ActionEntity, ent.Comp.Extended);
        _audio.PlayPredicted(ent.Comp.Extended ? ent.Comp.ExtendSound : ent.Comp.RetractSound, ent, ent);
    }

    #endregion

    #region Melee strikes

    private void OnGetMeleeWeapon(Entity<MantisBladeUserComponent> ent, ref GetMeleeWeaponEvent args)
    {
        if (args.Handled || !ent.Comp.Extended || ent.Comp.Blades.Count == 0)
            return;

        if (_hands.TryGetActiveItem(ent.Owner, out var held)
            && TryComp<MeleeWeaponComponent>(held, out var melee)
            && !melee.MustBeEquippedToUse)
            return;

        args.Weapon = ent.Comp.Blades[0];
        args.Handled = true;
    }

    private void OnGetMultihitWeapons(Entity<MantisBladeUserComponent> ent, ref MultihitGetWeaponsEvent args)
    {
        if (!ent.Comp.Extended || ent.Comp.Blades.Count == 0)
            return;

        if (TryComp<MultihitComponent>(ent.Comp.Blades[0], out var lead))
        {
            args.DamageMultiplier = lead.DamageMultiplier;
            args.Delay = lead.MultihitDelay;
        }

        if (!ent.Comp.Blades.Contains(args.Weapon))
            args.DamageMultiplier = ent.Comp.ArmedMultiplier;

        foreach (var blade in ent.Comp.Blades)
            if (blade != args.Weapon)
                args.Weapons.Add(blade);
    }

    #endregion
}
