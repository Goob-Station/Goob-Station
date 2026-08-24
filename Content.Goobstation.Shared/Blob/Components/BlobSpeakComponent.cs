// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Language;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Components;

//[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[RegisterComponent, NetworkedComponent]
public sealed partial class BlobSpeakComponent : Component
{
    [DataField]
    public ProtoId<LanguagePrototype> Language = "Blob";

    //[DataField, AutoNetworkedField]
    //public ProtoId<RadioChannelPrototype> Channel = "Hivemind";

    /// <summary>
    /// Hide entity name
    /// </summary>
    [DataField]
    public bool OverrideName = false; // Goob Edit, no overriding default name.

    [DataField]
    public LocId Name = "speak-vv-blob";
}
