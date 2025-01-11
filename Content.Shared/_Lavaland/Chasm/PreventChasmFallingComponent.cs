namespace Content.Shared._Lavaland.Chasm;

/// <summary>
/// Джаунтер
/// </summary>
[RegisterComponent]
public sealed partial class PreventChasmFallingComponent : Component
{
    [DataField]
    public bool DeleteOnUse = true;
}
