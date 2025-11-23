using Content.Goobstation.Common.Shuttles;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Popups;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Shuttles.Systems;

public sealed class GoobShuttleSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IFFConsoleComponent, IFFApplyRadarSettingsMessage>(OnIFFApplyRadarSettings);
        SubscribeLocalEvent<IFFConsoleComponent, GotEmaggedEvent>(OnGotEmagged);
        SubscribeLocalEvent<IFFConsoleComponent, IFFSettingsChangeAttemptEvent>(OnCheckCanChangeIFF);
    }

    public void OnGotEmagged(Entity<IFFConsoleComponent> entity, ref GotEmaggedEvent ev)
    {
        if (!_emag.CompareFlag(ev.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(entity.Owner, EmagType.Interaction))
            return;

        entity.Comp.AllowedFlags |= IFFFlags.Hide;
        entity.Comp.AllowedFlags |= IFFFlags.HideLabel;

        UpdateIIFInterface(entity);

        ev.Handled = true;
    }

    private void PopupOnStationIFFError(Entity<IFFConsoleComponent> entity)
    {
        _popup.PopupEntity(Loc.GetString("iff-console-station-iff-error"), entity);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg"), entity);

        UpdateIIFInterface(entity);
    }

    // ReSharper disable once InconsistentNaming
    private void UpdateIIFInterface(Entity<IFFConsoleComponent> entity)
    {
        var gridUid = Transform(entity).GridUid;

        if (!gridUid.HasValue)
            return;

        if (!EnsureComp<IFFComponent>(gridUid.Value, out var iff))
            return;

        _uiSystem.SetUiState(entity.Owner,
            IFFConsoleUiKey.Key,
            new IFFConsoleBoundUserInterfaceState()
        {
            AllowedFlags = entity.Comp.AllowedFlags,
            Flags = iff.Flags,
            Name = MetaData(gridUid.Value).EntityName,
            Color = iff.Color
        });
    }

    private void OnIFFApplyRadarSettings(Entity<IFFConsoleComponent> ent, ref IFFApplyRadarSettingsMessage args)
    {
        var xform = Transform(ent);
        if (xform.GridUid is null)
            return;

        var ev = new IFFSettingsChangeAttemptEvent();
        RaiseLocalEvent(ent, ref ev);
        if (!ev.CanChange)
            return;

        if (MetaData(xform.GridUid.Value).EntityName == args.Name && _shuttle.GetIFFColor(xform.GridUid.Value) == args.Color)
            return;

        args.Color.A = 1;
        _shuttle.SetIFFColor(xform.GridUid.Value, args.Color);

        _meta.SetEntityName(xform.GridUid.Value, args.Name is not null ? args.Name : "");

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Shuttle/radar_ping.ogg"), ent);
    }

    private void OnCheckCanChangeIFF(Entity<IFFConsoleComponent> ent, ref IFFSettingsChangeAttemptEvent args)
    {
        if (!HasComp<BecomesStationComponent>(ent))
            return;

        PopupOnStationIFFError(ent);
        args.CanChange = false;
    }
}
