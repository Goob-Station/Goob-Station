// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Server.Examine;
using Content.Server.Item;
using Content.Server.PowerCell;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.PowerCell.Components;
using Content.Shared.Storage;
using Content.Shared.Temperature;
using Content.Shared.Toggleable;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Heatlamp;

public sealed partial class HeatlampSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemSystem _item = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly ContainerSystem _container = default!;

    private readonly int _settingCount = Enum.GetValues<EntityHeaterSetting>().Length;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatlampComponent, EntGotInsertedIntoContainerMessage>(OnInsertedContainer);
        SubscribeLocalEvent<HeatlampComponent, EntGotRemovedFromContainerMessage>(OnRemovedContainer);

        SubscribeLocalEvent<HeatlampComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<HeatlampComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeatlampComponent>();
        while (query.MoveNext(out var uid, out var heater))
        {
            if (heater.User is not { } user
            || heater.Setting == EntityHeaterSetting.Off)
            {
                ToggleAbilities((uid, heater), false);
                continue;
            }

            ToggleAbilities((uid, heater), true);
            RegulateTemperature(user, (uid, heater), frameTime);
        }
    }

    private void ToggleAbilities(Entity<HeatlampComponent> heatlamp, bool enabled, AppearanceComponent? appearance = null, MeleeWeaponComponent? melee = null)
    {
        if (!Resolve(heatlamp, ref appearance, ref melee))
            return;

        if (enabled)
        {
            _item.SetSize(heatlamp, heatlamp.Comp.OnSize);
            _item.SetShape(heatlamp, heatlamp.Comp.OnShape);

            _pointLight.SetEnabled(heatlamp, true);

            _appearance.SetData(heatlamp, ToggleVisuals.Toggled, true, appearance);
            _item.SetHeldPrefix(heatlamp, "on");


            if (heatlamp.Comp.ActivatedDamage == null)
                return;

            heatlamp.Comp.DeactivatedDamage ??= melee.Damage;
            melee.Damage = heatlamp.Comp.ActivatedDamage;

        }
        else
        {
            _item.SetSize(heatlamp, heatlamp.Comp.OffSize);
            _item.SetShape(heatlamp, heatlamp.Comp.OffShape);

            _pointLight.SetEnabled(heatlamp, false);

            _appearance.SetData(heatlamp, ToggleVisuals.Toggled, false, appearance);
            _item.SetHeldPrefix(heatlamp, "off");

            if (heatlamp.Comp.DeactivatedDamage != null)
                melee.Damage = heatlamp.Comp.DeactivatedDamage;
        }
    }


    private void RegulateTemperature(
        EntityUid user,
        Entity<HeatlampComponent> heater,
        float frameTime,
        ThermalRegulatorComponent? regulator = null,
        TemperatureComponent? temperature = null,
        PowerCellSlotComponent? cell = null)
    {
        if (!Resolve(user, ref regulator, ref temperature) || !Resolve(heater, ref cell))
            return;

        var tempDelta = regulator.NormalBodyTemperature - temperature.CurrentTemperature;
        var deltaIsNegative = tempDelta < 0;

        var energy = heater.Comp.CurrentPowerDraw  * frameTime;

        if (heater.Comp.NeedsPower && !_powerCell.TryUseCharge(heater, energy, cell))
        {
            ChangeSetting((heater, heater), EntityHeaterSetting.Off);
            return;
        }

        if (heater.Comp.LowerEfficiencyWhenContained
            && _container.TryGetContainingContainer(heater.Owner, out var container)
            && HasComp<StorageComponent>(container.Owner))
        {
            energy *= heater.Comp.ContainerMultiplier;
        }


        var totalTransfer = energy * heater.Comp.PowerToHeatMultiplier;

        if (deltaIsNegative)
            totalTransfer *= heater.Comp.NegativeDeltaMultiplier;

        _temperature.ChangeHeat(user, totalTransfer, heater.Comp.ForceHeat);
    }
    private void OnInsertedContainer(Entity<HeatlampComponent> lamp, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!_inventory.TryGetContainingEntity(lamp.Owner, out var user))
            _inventory.TryGetContainingEntity(args.Container.Owner, out user);

        lamp.Comp.User = user;
    }

    private void OnRemovedContainer(Entity<HeatlampComponent> lamp, ref EntGotRemovedFromContainerMessage args)
    {
        lamp.Comp.User = null;
    }

    private void OnExamined(Entity<HeatlampComponent> lamp, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("entity-heater-examined", ("setting", lamp.Comp.Setting)));
    }

    private void OnGetVerbs(Entity<HeatlampComponent> lamp, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;

        var setting = (int) lamp.Comp.Setting;
        setting++;
        setting %= _settingCount;
        var nextSetting = (EntityHeaterSetting) setting;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("entity-heater-switch-setting", ("setting", nextSetting)),
            Act = () =>
            {
                ChangeSetting(lamp, nextSetting);
                _popup.PopupEntity(Loc.GetString("entity-heater-switched-setting", ("setting", nextSetting)), lamp, user);
            },
            Priority = -10,
        });
    }

    private void ChangeSetting(Entity<HeatlampComponent> lamp, EntityHeaterSetting setting)
    {
        lamp.Comp.Setting = setting;
        lamp.Comp.CurrentPowerDraw = SettingPower(setting, lamp.Comp.Power);

        _appearance.SetData(lamp, EntityHeaterVisuals.Setting, setting);
        _audio.PlayPvs(lamp.Comp.SettingSound, lamp);
    }

    private float SettingPower(EntityHeaterSetting setting, float max)
    {
        return setting switch
        {
            EntityHeaterSetting.Low => max / 3f,
            EntityHeaterSetting.Medium => max * 2f / 3f,
            EntityHeaterSetting.High => max,
            _ => 0f,
        };
    }
}
