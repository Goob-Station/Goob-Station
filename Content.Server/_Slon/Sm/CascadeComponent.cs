using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Server._Slon.Sm;

/// <summary>
/// Works only if the entity has the KudzuComponent
/// deals damage to anything in the radius, EVERYTHING.
/// </summary>
[RegisterComponent]
public sealed partial class CascadeComponent : Component
{
    [DataField]
    public float Radius = 1f;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 70 }
        }
    };

    [DataField]
    public float Interval = 1.0f; // seconds

    [ViewVariables]
    public float Timer = 0f; // timer to the next hit
}
