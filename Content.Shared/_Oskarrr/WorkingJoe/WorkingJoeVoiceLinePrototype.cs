// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.WorkingJoe;

/// <summary>
/// Voice synthesizer line for Working Joe / Engineer Jockey.
/// </summary>
[Prototype("workingJoeVoiceLine")]
public sealed partial class WorkingJoeVoiceLinePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Category = string.Empty;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public ProtoId<SoundCollectionPrototype> SoundCollection;
}
