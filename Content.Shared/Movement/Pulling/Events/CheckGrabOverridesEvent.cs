// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;

namespace Content.Shared.Movement.Pulling.Events;

public sealed class CheckGrabOverridesEvent : EntityEventArgs
{
    public CheckGrabOverridesEvent(GrabStage stage)
    {
        Stage = stage;
    }

    public GrabStage Stage { get; set; }
}