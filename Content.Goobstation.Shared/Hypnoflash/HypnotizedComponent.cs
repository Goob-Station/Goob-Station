namespace Content.Goobstation.Shared.Hypnoflash;

/// <summary>
/// given to the entity that will add the first thing it hears into their objectives
/// </summary>
[RegisterComponent]
public sealed partial class HypnotizedComponent : Component
{
    [DataField] public float Timer = 8; // you have 8 seconds to hear something or else you just dont do anything
}
