using System.Numerics;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Curses;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class CurseOfCorrosionStatusEffectComponent : Component
{
    [DataField, AutoPausedField]
    public TimeSpan NextVomit = TimeSpan.Zero;

    [DataField]
    public Vector2 MinMaxSecondsBetweenVomits = new(5f, 20f);

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "Poison", 0.3f },
        },
    };
}
