// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Actions;

namespace Content.Shared._Lavaland.Damage;

public sealed partial class HierophantClubActivateCrossEvent : WorldTargetActionEvent;

public sealed partial class HierophantClubPlaceMarkerEvent : InstantActionEvent;

public sealed partial class HierophantClubTeleportToMarkerEvent : InstantActionEvent;

public sealed partial class HierophantClubToggleTileMovementEvent : EntityTargetActionEvent;