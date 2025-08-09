// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Shared.Werewolf.Events;

#region Events - Pre-Werewolf
public sealed partial class WerewolfTransformEvent : InstantActionEvent;

public sealed partial class HeightenedSensesEvent : InstantActionEvent;
public sealed partial class OpenMutationShopEvent : InstantActionEvent;
#endregion

#region Events - Base Werewolf
public sealed partial class WerewolfGutEvent : EntityTargetActionEvent;
public sealed partial class WerewolfAmbushEvent : EntityTargetActionEvent;
public sealed partial class WerewolfHowlEvent : InstantActionEvent;
#endregion

#region Events - Do After
public sealed partial class WerewolfGutDoAfterEvent : SimpleDoAfterEvent;
#endregion
