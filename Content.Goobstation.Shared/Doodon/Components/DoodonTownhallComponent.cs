using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Doodon.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class DoodonTownhallComponent : Component
{
    /// <summary>
    /// The linked papa doodon that established this townhall.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid LinkedPapaDoodon = default!;
    /// <summary>
    /// The radius around the townhall in which the doodon buildings function. Doodon buildings are not supposed to function outside of the hall's influence.
    /// </summary>
    [DataField]
    public float InfluenceRadius = 7f;
    /// <summary>
    /// The collection of buildings linked to the townhall.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> LinkedDoodonBuildings = new();
}
