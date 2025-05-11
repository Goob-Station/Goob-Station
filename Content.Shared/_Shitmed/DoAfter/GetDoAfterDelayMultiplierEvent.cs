// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._Shitmed.DoAfter;

public sealed class GetDoAfterDelayMultiplierEvent(float multiplier = 1f) : EntityEventArgs, IBodyPartRelayEvent
{
    public float Multiplier = multiplier;

    public BodyPartType TargetBodyPart => BodyPartType.Hand;
}