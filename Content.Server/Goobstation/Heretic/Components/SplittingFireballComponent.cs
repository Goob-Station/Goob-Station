
namespace Content.Server.Heretic.Components;

[RegisterComponent]
public sealed partial class SplittingFireballComponent : Component
{
    public float Divisions = 0f;
    [DataField] public float MaxDivisions = 4f;
}
