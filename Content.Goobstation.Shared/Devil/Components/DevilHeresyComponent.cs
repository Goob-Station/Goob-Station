using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Devil.Components;

[RegisterComponent]
public sealed partial class DevilHeresyComponent : Component
{
    [DataField]
    public string RunePrototype = "HereticRuneRitual";

    [DataField]
    public string AnimationPrototype = "HereticRuneRitualDrawAnimation";

    [DataField]
    public float DrawTime = 14f;

    [ViewVariables]
    public float ElapsedTime = 0f;

    [ViewVariables]
    public EntityUid? AnimationEntity;
}
