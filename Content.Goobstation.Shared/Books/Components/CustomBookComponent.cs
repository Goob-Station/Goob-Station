using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Books;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CustomBookComponent : Component
{
    [DataField]
    public bool Template = true;

    [DataField]
    public Dictionary<string, (ResPath Path, string State)>? Binding;

    [DataField, AutoNetworkedField]
    public string Title = "";

    [DataField, AutoNetworkedField]
    public string Genre = "";

    [DataField, AutoNetworkedField]
    public string Desc = "";

    [DataField, AutoNetworkedField]
    public string Author = "";

    [DataField, AutoNetworkedField]
    public List<string> Pages = new();
}

[Serializable, NetSerializable]
public enum CustomBookUiKey : byte
{
    Key
}


[Serializable, NetSerializable]
public enum CustomBookVisuals : int
{
    Layer,
    Visuals
}
