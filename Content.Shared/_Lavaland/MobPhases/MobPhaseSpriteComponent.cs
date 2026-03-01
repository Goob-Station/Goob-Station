using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.MobPhases;

[RegisterComponent, NetworkedComponent]
public sealed partial class MobPhaseSpriteComponent : Component
{
    [DataField(required: true)]
    public Dictionary<int, PhaseSpriteData> Phases = new();
}

[DataRecord]
public sealed record PhaseSpriteData
{
    /// <summary>
    /// Whether this phase modifies the sprite at all.
    /// </summary>
    [DataField]
    public bool ChangeSprite = false;

    /// <summary>
    /// Optional RSI path override.
    /// </summary>
    [DataField]
    public string? Rsi;

    /// <summary>
    /// Optional sprite state override.
    /// </summary>
    [DataField]
    public string? State;

    /// <summary>
    /// Sprite layer to apply changes to.
    /// Defaults to base layer (0).
    /// </summary>
    [DataField]
    public int Layer = 0;
}
