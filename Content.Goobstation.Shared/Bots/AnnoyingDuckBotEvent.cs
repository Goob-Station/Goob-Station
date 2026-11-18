// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Bots;

/// <summary>
/// Raised on the annoying duck bot by the HTN when it reaches its chase target.
/// </summary>
[DataDefinition]
public sealed partial class AnnoyingDuckBotHonkEvent : EntityEventArgs
{
    [DataField]
    public EntityUid Target;
}
