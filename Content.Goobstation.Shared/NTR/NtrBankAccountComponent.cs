// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.NTR;

[RegisterComponent]
public sealed partial class NtrBankAccountComponent : Component
{
    [DataField]
    public int Balance;

    [DataField]
    public int TotalEarned;
}
