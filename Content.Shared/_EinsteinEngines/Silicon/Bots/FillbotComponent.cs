// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._EinsteinEngines.Silicon.Bots;

[RegisterComponent]
[Access(typeof(FillbotSystem))]
public sealed partial class FillbotComponent : Component
{
    [ViewVariables]
    public EntityUid? LinkedSinkEntity { get; set; }
}
