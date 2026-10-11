using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Paint;

[RegisterComponent, NetworkedComponent]
public sealed partial class PaintSurfaceComponent : Component
{
    [DataField]
    public Vector2i Size = new(64, 64);

    [DataField]
    public Color Background = Color.White;

    [DataField]
    public string Layer = "paint";

    [DataField]
    public bool Finished;

    [DataField]
    public string? Title;

    [DataField]
    public string? Artist;

    [ViewVariables]
    public PaintImage? Image;

    [ViewVariables]
    public GameTick[] ChunkModified = Array.Empty<GameTick>();

    [ViewVariables]
    public GameTick PaletteModified;
}

[Serializable, NetSerializable]
public sealed class PaintSurfaceComponentState(
    Vector2i size,
    Color[] palette,
    byte[] pixels,
    bool finished,
    string? title,
    string? artist)
    : ComponentState
{
    public Vector2i Size = size;
    public Color[] Palette = palette;
    public byte[] Pixels = pixels;
    public bool Finished = finished;
    public string? Title = title;
    public string? Artist = artist;
}

[Serializable, NetSerializable]
public sealed class PaintSurfaceComponentDeltaState(
    Color[]? palette,
    Dictionary<int, byte[]> chunks,
    bool finished,
    string? title,
    string? artist)
    : ComponentState, IComponentDeltaState<PaintSurfaceComponentState>
{
    public readonly Color[]? Palette = palette;
    public readonly Dictionary<int, byte[]> Chunks = chunks;
    public readonly bool Finished = finished;
    public readonly string? Title = title;
    public readonly string? Artist = artist;

    public void ApplyToFullState(PaintSurfaceComponentState state)
    {
        state.Finished = Finished;
        state.Title = Title;
        state.Artist = Artist;

        if (Palette != null)
            state.Palette = Palette;

        foreach (var (chunk, data) in Chunks)
        {
            PaintChunks.Write(state.Pixels, state.Size, chunk, data);
        }
    }

    public PaintSurfaceComponentState CreateNewFullState(PaintSurfaceComponentState state)
    {
        var newState = new PaintSurfaceComponentState(state.Size,
            state.Palette,
            (byte[]) state.Pixels.Clone(),
            state.Finished,
            state.Title,
            state.Artist);
        ApplyToFullState(newState);
        return newState;
    }
}

[ByRefEvent]
public readonly record struct PaintSurfaceUpdatedEvent(List<int>? Chunks);
