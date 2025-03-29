using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class EntropicPlumeAffectedComponent : Component
{
    [DataField]
    public float Duration = 10f;

    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextAttack = TimeSpan.Zero;

    [DataField]
    public SpriteSpecifier Sprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "cloud_swirl");
}

public enum EntropicPlumeKey : byte
{
    Key,
}
