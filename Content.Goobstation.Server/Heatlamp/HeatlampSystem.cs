using Content.Server.Examine;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Temperature;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Heatlamp;

public sealed partial class HeatlampSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    private readonly int _settingCount = Enum.GetValues<EntityHeaterSetting>().Length;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatlampComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<HeatlampComponent, GotUnequippedHandEvent>(OnUnequipped);

        SubscribeLocalEvent<HeatlampComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<HeatlampComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<HeatlampComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeatlampComponent, BatteryComponent>();
        while (query.MoveNext(out var uid, out var heater, out var battery))
        {
            if (heater.Setting == EntityHeaterSetting.Off
            || heater.NextTick >= _timing.CurTime)
                continue;

            if (heater.User is not null)
            {
                var energy = heater.CurrentPowerDraw * frameTime;
                _temperature.ChangeHeat(heater.User.Value, energy);
            }

            _battery.UseCharge(uid, heater.CurrentPowerDraw, battery);
            heater.NextTick = _timing.CurTime + heater.TickDelay;
        }
    }

    private void OnEquipped(EntityUid uid, HeatlampComponent comp, ref GotEquippedHandEvent args)
    {
        comp.User = args.User;
    }

    private void OnUnequipped(EntityUid uid, HeatlampComponent comp, ref GotUnequippedHandEvent args)
    {
        comp.User = null;
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
        comp.CurrentPowerDraw = SettingPower(setting, comp.Power);

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
