using System.Numerics;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VoidConduitComponent : Component
{
    [DataField]
    public int Range = 8;

    [DataField]
    public Vector2 MinMaxWindowDamageMultiplier = new(1f, 2f);

    [DataField]
    public Vector2 MinMaxAirlockDamageMultiplier = new(2f, 4f);

    [DataField]
    public DamageSpecifier StructureDamage = new()
    {
        DamageDict =
        {
            {"Structural", 30},
        },
    };

    [DataField]
    public SoundSpecifier WindowDamageSound = new SoundCollectionSpecifier("GlassSmack");

    [DataField]
    public SoundSpecifier AirlockDamageSound = new SoundPathSpecifier("/Audio/Weapons/smash.ogg");

    [DataField]
    public SpriteSpecifier OverlaySprite =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/void_overlay.rsi"), "voidtile");
}
