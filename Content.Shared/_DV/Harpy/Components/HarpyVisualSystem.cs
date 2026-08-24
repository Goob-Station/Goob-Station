// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._DV.Harpy.Components
{
    [Serializable, NetSerializable]
    public enum HarpyVisualLayers
    {
        Singing,
    }

    [Serializable, NetSerializable]
    public enum SingingVisualLayer
    {
        True,
        False,
    }
}