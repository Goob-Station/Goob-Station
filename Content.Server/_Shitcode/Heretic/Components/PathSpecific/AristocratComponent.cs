// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class AristocratComponent : Component
{
    [DataField] public float UpdateDelay = 0.1f;
    [DataField] public float Range = 10f;

    public int UpdateStep = 1;
    public float UpdateTimer = 0f;
    public bool HasDied = false;

    public SoundSpecifier VoidsEmbrace = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/VoidsEmbrace.ogg");
}
