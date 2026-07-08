// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Light;

public sealed class LightToggleEvent(bool isOn) : EntityEventArgs
{
    public bool IsOn = isOn;
}