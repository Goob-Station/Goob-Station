// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfMutationShopComponent : Component
{
    /// <summary>
    ///  The entity that is used to store the action,
    /// used for removing it once the user claims a form.
    /// </summary>
    [ViewVariables]
    public EntityUid? ActionEntity;
};

[NetSerializable, Serializable]
public enum MutationUiKey : byte
{
    Key
}
