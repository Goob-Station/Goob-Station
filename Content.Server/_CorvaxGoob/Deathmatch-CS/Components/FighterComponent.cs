namespace Content.Server._CorvaxGoob.Deathmatch_CS.Components;

[RegisterComponent]
public sealed partial class FighterComponent : Component
{
    /// <summary>
    ///     The component allows the CS system to recognize the mob as a participant in the battle.
    /// </summary>
    [DataField("isFighter"), ViewVariables(VVAccess.ReadWrite)]
    public bool IsFighter = true;

    [DataField("command"), ViewVariables(VVAccess.ReadWrite)]
    public int Command = 1;
}
