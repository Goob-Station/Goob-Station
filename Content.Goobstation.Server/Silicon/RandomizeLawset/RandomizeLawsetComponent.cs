using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Silicon.RandomizeLawset;

[RegisterComponent]
public sealed partial class RandomizeLawsetComponent : Component
{
    public string WeightedId;
}