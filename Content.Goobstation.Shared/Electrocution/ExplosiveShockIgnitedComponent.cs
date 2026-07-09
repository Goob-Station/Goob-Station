// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Electrocution;

[RegisterComponent]
public sealed partial class ExplosiveShockIgnitedComponent : Component
{
    public TimeSpan ExplodeAt;
}