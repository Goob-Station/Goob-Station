using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Silicon.MalfAI.Components;

[RegisterComponent]
public sealed partial class MalfStationAIHackableComponent : Component
{
    [DataField]
    public bool Hacked = false;

    [DataField]
    public float SecondsToHack = 10;
}