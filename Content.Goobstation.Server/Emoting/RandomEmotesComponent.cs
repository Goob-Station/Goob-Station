// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Emoting;

/// <summary>
/// Allows the entity to pick random emotes to say in the chat.
/// Tied to work with
/// </summary>
[RegisterComponent]
public sealed partial class RandomEmotesComponent : Component
{
    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> Weights;
}
