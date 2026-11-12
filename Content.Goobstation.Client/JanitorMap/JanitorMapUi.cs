using System.Numerics;
using Content.Client.Pinpointer.UI;
using Content.Client.UserInterface.Fragments;
using Content.Shared.Station;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;

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

        var map = new NavMapControl
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