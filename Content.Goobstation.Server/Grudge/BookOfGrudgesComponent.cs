using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;

namespace Content.Goobstation.Server.Grudge;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BookOfGrudgesComponent : Component
{
    [DataField]
    public List<string> Names = new List<string>();

    [DataField]
    public float GrudgeBookDamageModifier = 0.25f;

    [DataField]
    public float GrudgeCurseModifier = 1.25f;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Slash", 10 }
        }
    };

    // for book of Meta grudge. Ad meme. checks user name instead of character names
    [DataField]
    public bool MetaGrudge = false;
}
