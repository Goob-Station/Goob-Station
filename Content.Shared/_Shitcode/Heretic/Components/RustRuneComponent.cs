using System.Numerics;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class RustRuneComponent : Component
{
    /// <summary>
    /// If there is no rusted wall sprite - add rust overlay.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RustOverlay;

    [DataField]
    public ProtoId<TagPrototype> DiagonalTag = "Diagonal";

    [DataField, AutoNetworkedField]
    public Vector2 RuneOffset = Vector2.Zero;

    [DataField]
    public Vector2 DiagonalOffset = new(0.25f, -0.25f);

    [DataField]
    public SpriteSpecifier DiagonalSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "rust_diagonal");

    [DataField]
    public SpriteSpecifier OverlaySprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "rust_default");

    [DataField]
    public List<SpriteSpecifier> RuneSprites = new()
    {
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_1"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_2"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_3"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_4"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_5"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_6"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_7"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_8"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_9"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_10"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_11"),
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "small_rune_12"),
    };

    [DataField, AutoNetworkedField]
    public int RuneIndex;
}

public enum RustRuneKey : byte
{
    Rune,
    Overlay,
}
