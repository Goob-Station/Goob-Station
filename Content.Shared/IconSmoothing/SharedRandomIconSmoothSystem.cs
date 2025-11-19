// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.IconSmoothing;

public abstract class SharedRandomIconSmoothSystem : EntitySystem
{
}
[Serializable, NetSerializable]
public enum RandomIconSmoothState : byte
{
    State
}
