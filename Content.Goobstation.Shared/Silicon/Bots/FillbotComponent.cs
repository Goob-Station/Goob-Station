// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Silicon.Bots;

[RegisterComponent]
[Access(typeof(FillbotSystem))]
public sealed partial class FillbotComponent : Component
{
    [ViewVariables]
    public EntityUid? LinkedSinkEntity { get; set; }
}
