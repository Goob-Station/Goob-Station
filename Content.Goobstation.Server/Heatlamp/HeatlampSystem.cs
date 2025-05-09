// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Server.Item;
using Content.Server.PowerCell;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Temperature;
using Content.Shared.Verbs;
using Robust.Server.Audio;
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
                _item.SetSize(uid, heater.OffSize);
                _item.SetShape(uid, heater.OffShape);
                // set appearance here too
                continue;
            }

            _item.SetSize(uid, heater.OnSize);
            _item.SetShape(uid, heater.OnShape);
            RegulateTemperature(user, (uid, heater), frameTime);
        }
    }

    private void RegulateTemperature(
        EntityUid user,
        Entity<HeatlampComponent> heater,
        float frameTime,
        ThermalRegulatorComponent? regulator = null,
        TemperatureComponent? temperature = null)
    {
        if (!Resolve(user, ref regulator, ref temperature))
            return;

        var tempDelta = regulator.NormalBodyTemperature - temperature.CurrentTemperature;
        var deltaIsNegative = tempDelta < 0;

        var energy = heater.Comp.CurrentPowerDraw  * frameTime;

        if (!_powerCell.TryUseCharge(heater, energy * frameTime))
        {
            ChangeSetting((heater, heater), EntityHeaterSetting.Off);
            return;
        }

        if (heater.Comp.LowerEfficiencyWhenContained)
            energy /= heater.Comp.ContainerMultiplier;

        var totalTransfer = energy * heater.Comp.PowerToHeatMultiplier;

        if (deltaIsNegative)
            totalTransfer *= heater.Comp.NegativeDeltaMultiplier;

        _temperature.ChangeHeat(user, totalTransfer);
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
