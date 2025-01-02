namespace Content.Server._Lavaland.Tendril.Components;

/// <summary>
/// Моб, созданный тендрилом. При смерти удаляется из списка его спавнов
/// </summary>
[RegisterComponent]
public sealed partial class TendrilMobComponent : Component
{
    public EntityUid? Tendril;
}
