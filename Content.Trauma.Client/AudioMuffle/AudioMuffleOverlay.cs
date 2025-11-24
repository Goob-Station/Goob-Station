using System.Numerics;
using Content.Client.Resources;
using Content.Trauma.Shared.AudioMuffle;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Audio.Components;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Trauma.Client.AudioMuffle;

public sealed class AudioMuffleOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    private readonly AudioMuffleSystem _system;
    private readonly SharedTransformSystem _transform;
    private readonly SharedMapSystem _map;
    private readonly Font _font;

    public override OverlaySpace Space => OverlaySpace.WorldSpace | OverlaySpace.ScreenSpace;

    public AudioMuffleOverlay()
    {
        IoCManager.InjectDependencies(this);

        _system = _entManager.System<AudioMuffleSystem>();
        _transform = _entManager.System<SharedTransformSystem>();
        _map = _entManager.System<SharedMapSystem>();
        _font = _cache.GetFont("/Fonts/NotoSans/NotoSans-Regular.ttf", 12);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_system.ResolvePlayer() is not { } player)
            return;

        var playerPos = _transform.GetMapCoordinates(player);

        if (args.Space == OverlaySpace.ScreenSpace)
        {
            DrawTooltip(args, playerPos);
            return;
        }

        DrawData(args.WorldHandle, playerPos);
    }

    private void DrawData(DrawingHandleWorld handle, MapCoordinates playerPos)
    {
        if (_system.ResolvePlayerGrid(playerPos) is not { } grid)
            return;

        handle.SetTransform(_transform.GetWorldMatrix(grid));
        var offset = Vector2.One * 0.5f;
        foreach (var (indices, data) in _system.TileDataDict)
        {
            var color = indices != data.Indices ? Color.Red : Color.Green;
            color = color.WithAlpha(0.5f);
            var box = Box2.FromDimensions(new Vector2(indices.X, indices.Y), new Vector2(1, 1));
            handle.DrawRect(box, color);
            foreach (var next in data.Next)
            {
                handle.DrawLine(indices + offset, indices + offset + (next.Indices - indices) / 2f, Color.Yellow);
            }

            if (data.Previous == null)
                continue;

            handle.DrawLine(indices + offset, indices + offset + (data.Previous.Indices - indices) / 2f, Color.Blue);
        }

        handle.SetTransform(Matrix3x2.Identity);
    }

    private void DrawTooltip(in OverlayDrawArgs args, MapCoordinates playerPos)
    {
        var handle = args.ScreenHandle;
        var mousePos = _input.MouseScreenPosition;
        if (!mousePos.IsValid)
            return;

        if (_ui.MouseGetControl(mousePos) is not IViewportControl viewport)
            return;

        var coords = viewport.PixelToMap(mousePos.Position);
        if (_system.TryFindCommonPlayerGrid(playerPos, coords) is not { } grid)
            return;

        var index = _map.TileIndicesFor(grid, coords);
        DrawTooltip(handle, mousePos.Position, index);
    }

    private void DrawTooltip(DrawingHandleScreen handle, Vector2 pos, Vector2i index)
    {
        var lineHeight = _font.GetLineHeight(1f);
        var offset = new Vector2(0, lineHeight);
        var offset2 = new Vector2(lineHeight, 0);

        var blockerSet =
            _system.ReverseBlockerIndicesDict.GetValueOrDefault(index, new HashSet<Entity<SoundBlockerComponent>>());
        var blockersCount = blockerSet.Count;
        var audioSet = _system.ReverseAudioPosDict.GetValueOrDefault(index, new HashSet<Entity<AudioComponent, AudioMuffleComponent>>());
        var audioCount = audioSet.Count;
        var hasTileData = _system.TileDataDict.TryGetValue(index, out var data);

        handle.DrawString(_font, pos, $"Indices: {index}");
        pos += offset;
        handle.DrawString(_font, pos, $"Blockers amount: {blockersCount}");
        pos += offset;
        handle.DrawString(_font, pos, $"Audio count: {audioCount}");

        if (hasTileData)
        {
            pos += offset;
            handle.DrawString(_font, pos, "Tile data:");
            pos += offset2;
            pos += offset;
            handle.DrawString(_font, pos, $"Indices: {data!.Indices}");
            pos += offset;
            handle.DrawString(_font, pos, $"Total cost: {data.TotalCost}");
            if (data.Previous != null)
            {
                pos += offset;
                handle.DrawString(_font, pos, $"Previous: {data.Previous.Indices}");
            }

            if (data.Next.Count <= 0)
                return;

            pos += offset;
            handle.DrawString(_font, pos, "Next:");
            pos += offset2;
            foreach (var next in data.Next)
            {
                pos += offset;
                handle.DrawString(_font, pos, $"Indices: {next.Indices}");
            }
        }

        pos -= offset2;

        if (audioCount <= 0)
            return;

        pos += offset;
        handle.DrawString(_font, pos, "Audio data:");
        pos += offset2;
        foreach (var audio in audioSet)
        {
            pos += offset;
            var volumeStr = $"{audio.Comp2.OriginalVolume:0.00}";
            var realVolumeStr = $"{audio.Comp1.Params.Volume:0.00}";

            handle.DrawString(_font, pos, $"Volume: {volumeStr}");
            pos += offset;
            handle.DrawString(_font, pos, $"Real volume: {realVolumeStr}");
        }
    }
}
