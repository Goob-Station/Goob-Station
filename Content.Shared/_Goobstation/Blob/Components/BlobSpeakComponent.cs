//using Content.Shared.Language;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Blob.Components;

//[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[RegisterComponent, NetworkedComponent]
public sealed partial class BlobSpeakComponent : Component
{
  //  [DataField]
    //public ProtoId<LanguagePrototype> Language = "Blob";

    //[DataField, AutoNetworkedField]
    //public ProtoId<RadioChannelPrototype> Channel = "Hivemind";

    /// <summary>
    /// Hide entity name
    /// </summary>
    [DataField]
    public bool OverrideName = true;

    [DataField]
    public LocId Name = "speak-vv-blob";
}
