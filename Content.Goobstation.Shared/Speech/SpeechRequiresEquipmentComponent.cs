// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Speech;

/// <summary>
/// Makes it so you can't speak if you don't have a valid item in the specified slot(s)
/// </summary>
[RegisterComponent]
[Access(typeof(SpeechRequiresEquipmentSystem))]
public sealed partial class SpeechRequiresEquipmentComponent : Component
{
    /// <summary>
    /// What inventory slots should have what items for you to be able to speak
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, EntityWhitelist> Equipment = [];

    /// <summary>
    /// The message to display when you fail to speak
    /// </summary>
    [DataField]
    public LocId? FailMessage;
}
