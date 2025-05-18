// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.MisandryBox;

[ByRefEvent]
public record struct BeforeIgniteFirestacksEvent(EntityUid Entity, float FireStacks);
