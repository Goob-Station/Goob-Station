// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.PDA
{
    [Serializable, NetSerializable]
    public enum PdaVisuals
    {
        IdCardInserted,
        PenInserted,//goob addition for pen visual
        PdaType
    }

    [Serializable, NetSerializable]
    public enum PdaUiKey
    {
        Key
    }

}
