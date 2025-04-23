// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Goobstation.Shared.Temperature;

public sealed class TemperatureImmunityEvent : EntityEventArgs
{
    public bool IsImmune = false; // completely immune to temperature?
    public bool HighImmune = false; // immune to HIGH temperature?
    public bool LowImmune = false; // immune to LOW temperature?
    public readonly float IdealTemperature = Atmospherics.T37C;
}
