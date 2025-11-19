// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Construction;

[Serializable, NetSerializable]
public enum ConstructionVisuals : byte
{
    Key,
    Layer,
    Wired,
}
