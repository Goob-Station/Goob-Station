using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Personal;

[Prototype("personalItems")]
public sealed class PersonalEntityPrototype : IPrototype
{
    /// <summary>
    /// ID's for prototype should be named username + "Personal". Example: "NikitosPersonal" if user's name Nikitos.
    /// </summary>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// List of entities to spawn.
    /// </summary>
    [DataField]
    public List<EntProtoId> ItemList { get; private set; } = new();
}
