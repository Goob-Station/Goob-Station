// SPDX-FileCopyrightText: 2024 PotentiallyTom <67602105+PotentiallyTom@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Goidastation.Security;

[Serializable, NetSerializable]
public sealed partial class PanicButtonDoAfterEvent : SimpleDoAfterEvent
{
}
