// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.MisandryBox.Grass;

// Called when minutes are added to a client
public delegate Task AddedPlaytimeMinutesCallback(ICommonSession player, int minutes);
