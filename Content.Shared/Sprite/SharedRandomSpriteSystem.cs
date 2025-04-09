// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Sprite;

public abstract class SharedRandomSpriteSystem : EntitySystem {}

[Serializable, NetSerializable]
public sealed class RandomSpriteColorComponentState : ComponentState
{
    public Dictionary<string, (string State, Color? Color)> Selected = default!;
}