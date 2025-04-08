// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Condemned;

public sealed class CondemnedCompleteEvent : EntityEventArgs
{
    public EntityUid CondemnedEntity;
}
