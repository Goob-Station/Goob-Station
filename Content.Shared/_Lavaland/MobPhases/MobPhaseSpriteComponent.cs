using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.MobPhases;

[RegisterComponent, NetworkedComponent]
public sealed partial class MobPhaseSpriteComponent : Component
{
    [DataField(required: true)]
    public Dictionary<int, PhaseSpriteData> Phases = new();
}

[DataRecord]
public sealed partial record PhaseSpriteData
{
    /// <summary>
    /// Should this phase switch modify the sprite?
    /// </summary>
    [DataField]
    public bool ChangeSprite = false;

    /// <summary>
    /// In case your RSI path for each phase is different.
    /// </summary>
    [DataField]
    public string? Rsi;

    /// <summary>
    /// The sprite name within the RSI.
    /// </summary>
    [DataField]
    public string? State;

    /// <summary>
    /// Sprite layer to change.
    /// </summary>
    [DataField]
    public int Layer = 0;
}
