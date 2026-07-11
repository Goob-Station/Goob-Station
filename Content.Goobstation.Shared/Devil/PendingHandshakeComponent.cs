// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Devil;

[RegisterComponent]
public sealed partial class PendingHandshakeComponent : Component
{
    [DataField]
    public EntityUid? Offerer;

    [DataField]
    public TimeSpan ExpiryTime;
}
