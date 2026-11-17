using System.Numerics;
using Content.Client.Pinpointer.UI;
using Content.Client.UserInterface.Fragments;
using Content.Goobstation.Shared.JanitorCallButton;
using Content.Shared.Light.Components;
using Content.Shared.Station;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.JanitorMap;

public sealed partial class JanitorMapUi : UIFragment
{
    private JanitorMapUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        var entities = IoCManager.Resolve<IEntityManager>();
        var player = IoCManager.Resolve<IPlayerManager>().LocalEntity;
        _fragment = new JanitorMapUiFragment(entities, player);
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
    }
}

public sealed class JanitorMapUiFragment : BoxContainer
{
    public JanitorMapUiFragment(IEntityManager entities, EntityUid? player)
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        VerticalExpand = true;
        RectClipContent = true;

        var map = new JanitorMapNavMapControl(entities)
        {
            SetSize = new Vector2(540, 340),
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        AddChild(map);

        if (player is not { } playerEntity || !entities.TryGetComponent<TransformComponent>(playerEntity, out var transform))
            return;

        var stationSystem = entities.System<SharedStationSystem>();
        var station = stationSystem.GetOwningStation(playerEntity, transform);
        var grid = station != null
            ? stationSystem.GetLargestGrid((station.Value, null))
            : transform.GridUid != EntityUid.Invalid ? transform.GridUid : null;

        map.MapUid = grid;
        if (grid == null)
            return;

        map.TrackedCoordinates.Add(new EntityCoordinates(playerEntity, Vector2.Zero), (true, Color.Cyan));
        map.ForceNavMapUpdate();
    }
}

public sealed class JanitorMapNavMapControl : NavMapControl
{
    private readonly IEntityManager _entities;
    private readonly SpriteSystem _spriteSystem;
    private readonly Texture _blipTexture;

    public JanitorMapNavMapControl(IEntityManager entities)
    {
        _entities = entities;
        _spriteSystem = entities.System<SpriteSystem>();
        _blipTexture = _spriteSystem.Frame0(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/NavMap/beveled_circle.png")));
    }

    protected override void UpdateNavMap()
    {
        base.UpdateNavMap();
        TrackedEntities.Clear();

        if (MapUid is not { } grid)
            return;

        var query = _entities.EntityQueryEnumerator<JanitorCallButtonComponent, SlimPoweredLightComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var light, out var transform))
        {
            if (transform.GridUid != grid || !_entities.TryGetNetEntity(uid, out var netEntity) || netEntity == null)
                continue;

            var color = light.Enabled ? Color.Red : Color.Green;
            TrackedEntities[netEntity.Value] = new NavMapBlip(new EntityCoordinates(uid, Vector2.Zero), _blipTexture, color, false, false);
        }
    }
}
