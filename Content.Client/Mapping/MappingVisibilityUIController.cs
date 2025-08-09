// SPDX-FileCopyrightText: 2025 ReserveBot <211949879+ReserveBot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Svarshik <96281939+lexaSvarshik@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 poemota <142114334+poeMota@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

/// Reserve - File heavily edited by PR: Mapping editor.
/// See https://github.com/space-wizards/space-station-14/pull/34302
/// and https://github.com/Reserve-Station/Reserve-Station/pull/82 for more details.

using Content.Client.Decals;
using Content.Client.Markers;
using Content.Client.SubFloor;
using Content.Shared.Atmos.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Tag;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Client.UserInterface;

namespace Content.Client.Mapping;

public sealed class MappingVisibilityUIController : UIController
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private MappingVisibilityWindow? _window;
    private MappingScreen? _mappingScreen; // WD EDIT

    [ValidatePrototypeId<TagPrototype>]
    private const string WallTag = "Wall";

    [ValidatePrototypeId<TagPrototype>]
    private const string CableTag = "Cable";

    [ValidatePrototypeId<TagPrototype>]
    private const string DisposalTag = "Disposal";

    // WD EDIT START
    private bool _entitiesVisible = true;
    private bool _tilesVisible = true;
    private bool _decalsVisible = true;
    // WD EDIT END

    public void ToggleWindow()
    {
        EnsureWindow();

        if (_window!.IsOpen)
        {
            _window.Close();
        }
        else
        {
            _window.Open();
        }
    }

    private void EnsureWindow()
    {
        if (_window is { Disposed: false })
            return;

        _window = UIManager.CreateWindow<MappingVisibilityWindow>();
        // WD EDIT START
        _mappingScreen = UIManager.ActiveScreen as MappingScreen;

        _window.EntitiesPanel.Pressed = _entitiesVisible;
        _window.EntitiesPanel.OnPressed += OnToggleEntitiesPanelPressed;

        _window.TilesPanel.Pressed = _tilesVisible;
        _window.TilesPanel.OnPressed += OnToggleTilesPanelPressed;

        _window.DecalsPanel.Pressed = _decalsVisible;
        _window.DecalsPanel.OnPressed += OnToggleDecalsPanelPressed;
        // WD EDIT END

        _window.Light.Pressed = _lightManager.Enabled;
        _window.Light.OnPressed += args => _lightManager.Enabled = args.Button.Pressed;

        _window.Fov.Pressed = _eyeManager.CurrentEye.DrawFov;
        _window.Fov.OnPressed += args => _eyeManager.CurrentEye.DrawFov = args.Button.Pressed;

        _window.Shadows.Pressed = _lightManager.DrawShadows;
        _window.Shadows.OnPressed += args => _lightManager.DrawShadows = args.Button.Pressed;

        _window.Entities.Pressed = true;
        _window.Entities.OnPressed += OnToggleEntitiesLayerPressed; // WD EDIT - OnToggleEntitiesPressed -> OnToggleEntitiesLayerPressed

        _window.Markers.Pressed = _entitySystemManager.GetEntitySystem<MarkerSystem>().MarkersVisible;
        _window.Markers.OnPressed += args =>
        {
            _entitySystemManager.GetEntitySystem<MarkerSystem>().MarkersVisible = args.Button.Pressed;
        };

        _window.Walls.Pressed = true;
        _window.Walls.OnPressed += args => ToggleWithTag(args, WallTag);

        _window.Airlocks.Pressed = true;
        _window.Airlocks.OnPressed += ToggleWithComp<AirlockComponent>;

        _window.Decals.Pressed = true;
        _window.Decals.OnPressed += OnToggleDecalsLayerPressed; // WD EDIT

        _window.SubFloor.Pressed = _entitySystemManager.GetEntitySystem<SubFloorHideSystem>().ShowAll;
        _window.SubFloor.OnPressed += OnToggleSubfloorPressed;

        _window.Cables.Pressed = true;
        _window.Cables.OnPressed += args => ToggleWithTag(args, CableTag);

        _window.Disposal.Pressed = true;
        _window.Disposal.OnPressed += args => ToggleWithTag(args, DisposalTag);

        _window.Atmos.Pressed = true;
        _window.Atmos.OnPressed += ToggleWithComp<PipeAppearanceComponent>;

        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);
    }

    // WD EDIT START
    private void OnToggleEntitiesPanelPressed(BaseButton.ButtonEventArgs args)
    {
        _entitiesVisible = args.Button.Pressed;

        if (_mappingScreen == null)
            return;

        _mappingScreen.SpawnContainer.Visible = args.Button.Pressed;
    }

    private void OnToggleTilesPanelPressed(BaseButton.ButtonEventArgs args)
    {
        _tilesVisible = args.Button.Pressed;

        if (_mappingScreen == null)
            return;

        _mappingScreen.TilesPanel.Visible = args.Button.Pressed;
        UpdatePanelContainerVisibility();
    }

    private void OnToggleDecalsPanelPressed(BaseButton.ButtonEventArgs args)
    {
        _decalsVisible = args.Button.Pressed;

        if (_mappingScreen == null)
            return;

        _mappingScreen.DecalsPanel.Visible = args.Button.Pressed;
        _mappingScreen.DecalSettings.Visible = args.Button.Pressed;
        UpdatePanelContainerVisibility();
    }

    private void UpdatePanelContainerVisibility()
    {
        if (_mappingScreen == null)
            return;

        var isVisible = _decalsVisible || _tilesVisible;
        _mappingScreen.PanelContainer.Visible = isVisible;

        // dividing line management
        if (_mappingScreen.TileDecalSeparator != null)
        {
            _mappingScreen.TileDecalSeparator.Visible = _tilesVisible && _decalsVisible;
        }
         // WD EDIT END
    }

    private void OnToggleEntitiesLayerPressed(BaseButton.ButtonEventArgs args)  // WD EDIT - OnToggleEntitiesPressed -> OnToggleEntitiesLayerPressed
    {
        var query = _entityManager.AllEntityQueryEnumerator<SpriteComponent>();

        if (args.Button.Pressed && _window != null)
        {
            _window.Markers.Pressed = true;
            _window.Walls.Pressed = true;
            _window.Airlocks.Pressed = true;
        }
        else if (_window != null)
        {
            _window.Markers.Pressed = false;
            _window.Walls.Pressed = false;
            _window.Airlocks.Pressed = false;
        }

        while (query.MoveNext(out _, out var sprite))
        {
            sprite.Visible = args.Button.Pressed;
        }
    }

    // WD EDIT START
    private void OnToggleDecalsLayerPressed(BaseButton.ButtonEventArgs args)
    {
        _entitySystemManager.GetEntitySystem<DecalSystem>().ToggleOverlay();
    }
    // WD EDIT END

    private void OnToggleSubfloorPressed(BaseButton.ButtonEventArgs args)
    {
        _entitySystemManager.GetEntitySystem<SubFloorHideSystem>().ShowAll = args.Button.Pressed;

        if (args.Button.Pressed && _window != null)
        {
            _window.Cables.Pressed = true;
            _window.Atmos.Pressed = true;
            _window.Disposal.Pressed = true;
        }
    }

    private void ToggleWithComp<TComp>(BaseButton.ButtonEventArgs args) where TComp : IComponent
    {
        var query = _entityManager.AllEntityQueryEnumerator<TComp, SpriteComponent>();

        while (query.MoveNext(out _, out _, out var sprite))
        {
            sprite.Visible = args.Button.Pressed;
        }
    }

    private void ToggleWithTag(BaseButton.ButtonEventArgs args, ProtoId<TagPrototype> tag)
    {
        var query = _entityManager.AllEntityQueryEnumerator<TagComponent, SpriteComponent>();
        var tagSystem = _entityManager.EntitySysManager.GetEntitySystem<TagSystem>();

        while (query.MoveNext(out var uid, out _, out var sprite))
        {
            if (tagSystem.HasTag(uid, tag))
                sprite.Visible = args.Button.Pressed;
        }
    }
}
