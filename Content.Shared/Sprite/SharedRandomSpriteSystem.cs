// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Sprite;

public abstract class SharedRandomSpriteSystem : EntitySystem {}

[Serializable, NetSerializable]
public sealed class RandomSpriteColorComponentState : ComponentState
{
    public Dictionary<string, (string State, Color? Color)> Selected = default!;
}
