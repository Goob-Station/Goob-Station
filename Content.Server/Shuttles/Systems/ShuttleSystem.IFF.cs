// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Shuttles.Components;
using Content.Server.Station.Components;
using Content.Shared.CCVar;
using Content.Shared.Emag.Systems;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Events;
using Robust.Shared.Audio;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private void InitializeIFF()
    {
        SubscribeLocalEvent<IFFConsoleComponent, AnchorStateChangedEvent>(OnIFFConsoleAnchor);
        SubscribeLocalEvent<IFFConsoleComponent, IFFShowIFFMessage>(OnIFFShow);
        SubscribeLocalEvent<IFFConsoleComponent, IFFShowVesselMessage>(OnIFFShowVessel);

        SubscribeLocalEvent<IFFConsoleComponent, IFFApplyRadarSettingsMessage>(OnIFFApplyRadarSettings); // CorvaxGoob-IIF-Improves

        SubscribeLocalEvent<IFFConsoleComponent, GotEmaggedEvent>(OnGotEmagged); // CorvaxGoob-IIF-Improves

        SubscribeLocalEvent<GridSplitEvent>(OnGridSplit);
    }

    private void OnGridSplit(ref GridSplitEvent ev)
    {
        var splitMass = _cfg.GetCVar(CCVars.HideSplitGridsUnder);

        if (splitMass < 0)
            return;

        foreach (var grid in ev.NewGrids)
        {
            if (!_physicsQuery.TryGetComponent(grid, out var physics) ||
                physics.Mass > splitMass)
            {
                continue;
            }

            AddIFFFlag(grid, IFFFlags.HideLabel);
        }
    }

    // CorvaxGoob-IIF-Improves-Start
    public void OnGotEmagged(Entity<IFFConsoleComponent> entity, ref GotEmaggedEvent ev)
    {
        if (!_emag.CompareFlag(ev.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(entity.Owner, EmagType.Interaction))
            return;

        entity.Comp.AllowedFlags |= IFFFlags.Hide;
        entity.Comp.AllowedFlags |= IFFFlags.HideLabel;

        UpdateIIFInerface(entity);

        ev.Handled = true;
    }

    private void PopupOnStationIFFError(Entity<IFFConsoleComponent> entity)
    {
        _popup.PopupEntity(Loc.GetString("iff-console-station-iff-error"), entity);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg"), entity);

        UpdateIIFInerface(entity);
    }

    private void UpdateIIFInerface(Entity<IFFConsoleComponent> entity)
    {
        var gridUid = Transform(entity).GridUid;

        if (!gridUid.HasValue)
            return;

        if (!EnsureComp<IFFComponent>(gridUid.Value, out var iff))
            return;

        _uiSystem.SetUiState(entity.Owner, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
        {
            AllowedFlags = entity.Comp.AllowedFlags,
            Flags = iff.Flags,
            Name = MetaData(gridUid.Value).EntityName,
            Color = iff.Color
        });
    }

    private void OnIFFApplyRadarSettings(Entity<IFFConsoleComponent> entity, ref IFFApplyRadarSettingsMessage args)
    {
        if (!TryComp(entity, out TransformComponent? xform) || xform.GridUid is null)
            return;

        if (HasComp<BecomesStationComponent>(xform.GridUid))
        {
            PopupOnStationIFFError(entity);
            return;
        }

        if (MetaData(xform.GridUid.Value).EntityName == args.Name && GetIFFColor(xform.GridUid.Value) == args.Color)
            return;

        args.Color.A = 1;
        SetIFFColor(xform.GridUid.Value, args.Color);

        _metadata.SetEntityName(xform.GridUid.Value, args.Name is not null ? args.Name : "");

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Shuttle/radar_ping.ogg"), entity);
    }
    // CorvaxGoob-IIF-Improves-End

    private void OnIFFShow(EntityUid uid, IFFConsoleComponent component, IFFShowIFFMessage args)
    {
        if (!TryComp(uid, out TransformComponent? xform) || xform.GridUid == null ||
            (component.AllowedFlags & IFFFlags.HideLabel) == 0x0)
        {
            return;
        }

        if (HasComp<BecomesStationComponent>(xform.GridUid)) // CorvaxGoob-IIF-Improves
        {
            PopupOnStationIFFError((uid, component));
            return;
        }

        if (!args.Show)
        {
            AddIFFFlag(xform.GridUid.Value, IFFFlags.HideLabel);
        }
        else
        {
            RemoveIFFFlag(xform.GridUid.Value, IFFFlags.HideLabel);
        }
    }

    private void OnIFFShowVessel(EntityUid uid, IFFConsoleComponent component, IFFShowVesselMessage args)
    {
        if (!TryComp(uid, out TransformComponent? xform) || xform.GridUid == null ||
            (component.AllowedFlags & IFFFlags.Hide) == 0x0)
        {
            return;
        }

        if (HasComp<BecomesStationComponent>(xform.GridUid)) // CorvaxGoob-IIF-Improves
        {
            PopupOnStationIFFError((uid, component));
            return;
        }

        if (!args.Show)
        {
            AddIFFFlag(xform.GridUid.Value, IFFFlags.Hide);
        }
        else
        {
            RemoveIFFFlag(xform.GridUid.Value, IFFFlags.Hide);
        }
    }

    private void OnIFFConsoleAnchor(EntityUid uid, IFFConsoleComponent component, ref AnchorStateChangedEvent args)
    {
        if (!TryComp(uid, out TransformComponent? xform)) // CorvaxGoob-IIF-Improves
            return;

        // If we anchor / re-anchor then make sure flags up to date.
        if (!args.Anchored ||
            !TryComp<IFFComponent>(xform.GridUid, out var iff)) // CorvaxGoob-IFF-Changes : removed !TryComp(uid, out TransformComponent? xform)
        {
            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = component.AllowedFlags,
                Flags = IFFFlags.None,
                Name = xform.GridUid.HasValue ? MetaData(xform.GridUid.Value).EntityName : null, // CorvaxGoob-IIF-Improves
                Color = Color.Gold // CorvaxGoob-IIF-Improves
            });
        }
        else
        {
            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = component.AllowedFlags,
                Flags = iff.Flags,
                Name = MetaData(xform.GridUid.Value).EntityName, // CorvaxGoob-IIF-Improves
                Color = iff.Color // CorvaxGoob-IIF-Improves
            });
        }
    }

    protected override void UpdateIFFInterfaces(EntityUid gridUid, IFFComponent component)
    {
        base.UpdateIFFInterfaces(gridUid, component);

        var query = AllEntityQuery<IFFConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (xform.GridUid != gridUid)
                continue;

            _uiSystem.SetUiState(uid, IFFConsoleUiKey.Key, new IFFConsoleBoundUserInterfaceState()
            {
                AllowedFlags = comp.AllowedFlags,
                Flags = component.Flags,
                Name = MetaData(gridUid).EntityName, // CorvaxGoob-IIF-Improves
                Color = component.Color // CorvaxGoob-IIF-Improves
            });
        }
    }
}
