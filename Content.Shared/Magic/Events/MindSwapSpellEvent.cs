// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Audio;

namespace Content.Shared.Magic.Events;

public sealed partial class MindSwapSpellEvent : EntityTargetActionEvent
{
    [DataField]
    public TimeSpan PerformerStunDuration = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan TargetStunDuration = TimeSpan.FromSeconds(10);

    // Goobstation
    [DataField]
    public SoundSpecifier? Sound;
}