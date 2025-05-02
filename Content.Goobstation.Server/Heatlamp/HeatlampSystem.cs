using Content.Server.Power.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Temperature;
using Content.Shared.Verbs;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.Heatlamp;

public sealed partial class HeatlampSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    private readonly int _settingCount = Enum.GetValues<EntityHeaterSetting>().Length;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatlampComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<HeatlampComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnExamined(EntityUid uid, HeatlampComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("entity-heater-examined", ("setting", comp.Setting)));
    }

    private void OnGetVerbs(EntityUid uid, HeatlampComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var setting = (int) comp.Setting;
        setting++;
        setting %= _settingCount;
        var nextSetting = (EntityHeaterSetting) setting;

        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("entity-heater-switch-setting", ("setting", nextSetting)),
            Act = () =>
            {
                ChangeSetting(uid, nextSetting, comp);
                _popup.PopupEntity(Loc.GetString("entity-heater-switched-setting", ("setting", nextSetting)), uid, args.User);
            }
        });
    }

    private void OnPowerChanged(EntityUid uid, HeatlampComponent comp, ref PowerChangedEvent args)
    {
        var setting = args.Powered ? comp.Setting : EntityHeaterSetting.Off;
        _appearance.SetData(uid, EntityHeaterVisuals.Setting, setting);
    }

    private void ChangeSetting(EntityUid uid, EntityHeaterSetting setting, HeatlampComponent? comp = null, BatteryComponent? power = null)
    {
        if (!Resolve(uid, ref comp, ref power))
            return;

        comp.Setting = setting;
        comp.CurrentPowerDraw = SettingPower(setting, power.MaxCharge);

        _appearance.SetData(uid, EntityHeaterVisuals.Setting, setting);
        _audio.PlayPvs(comp.SettingSound, uid);
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
