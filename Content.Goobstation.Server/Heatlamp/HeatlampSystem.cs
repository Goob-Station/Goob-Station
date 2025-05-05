using Content.Server.Body.Components;
using Content.Server.Examine;
using Content.Server.PowerCell;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
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

    private readonly int _settingCount = Enum.GetValues<EntityHeaterSetting>().Length;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatlampComponent, GotEquippedHandEvent>(OnEquippedHand);
        SubscribeLocalEvent<HeatlampComponent, GotUnequippedHandEvent>(OnUnequippedHand);

        SubscribeLocalEvent<HeatlampComponent, EntGotInsertedIntoContainerMessage>(OnInsertedContainer);
        SubscribeLocalEvent<HeatlampComponent, EntGotRemovedFromContainerMessage>(OnRemovedContainer);

        SubscribeLocalEvent<HeatlampComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<HeatlampComponent, GotUnequippedEvent>(OnUnequipped);

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
            || heater.Setting == EntityHeaterSetting.Off
            || !TryComp<TemperatureComponent>(user, out var temperature)
            || !TryComp<ThermalRegulatorComponent>(user, out var thermalRegulator))
                continue;

            var tempDelta = thermalRegulator.NormalBodyTemperature - temperature.CurrentTemperature;
            if (tempDelta <= 0)
                continue;

            var energy = heater.CurrentPowerDraw  * frameTime;
            var energyToUse = energy / 4f;

            if (!_powerCell.HasCharge(uid, energyToUse))
            {
                ChangeSetting((uid, heater), EntityHeaterSetting.Off);
                return;
            }

            if (heater.LowerEfficiencyWhenContained)
                energy /= heater.ContainerMultiplier;

            _temperature.ChangeHeat(user, energy * heater.PowerToHeatMultiplier);
            _powerCell.TryUseCharge(uid, energyToUse);
        }
    }

    private void OnEquippedHand(Entity<HeatlampComponent> lamp, ref GotEquippedHandEvent args)
    {
        lamp.Comp.User = args.User;
    }

    private void OnUnequippedHand(Entity<HeatlampComponent> lamp, ref GotUnequippedHandEvent args)
    {
        lamp.Comp.User = null;
    }

    private void OnEquipped(Entity<HeatlampComponent> lamp, ref GotEquippedEvent args)
    {
        lamp.Comp.User = args.Equipee;
    }

    private void OnUnequipped(Entity<HeatlampComponent> lamp, ref GotUnequippedEvent args)
    {
        lamp.Comp.User = args.Equipee;
    }

    private void OnInsertedContainer(Entity<HeatlampComponent> lamp, ref EntGotInsertedIntoContainerMessage args)
    {
        _inventory.TryGetContainingEntity(args.Container.Owner, out lamp.Comp.User);
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
