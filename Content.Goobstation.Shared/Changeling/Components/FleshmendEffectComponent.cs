using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
///     Component responsible for Fleshmend's visual effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FleshmendEffectComponent : Component
{
    [DataField]
    public string EffectState = "mend_active";

    [DataField]
    public ResPath ResPath = new("_Goobstation/Changeling/fleshmend_visuals.rsi");

}

public enum FleshmendEffectKey : byte
{
    Key,
}
