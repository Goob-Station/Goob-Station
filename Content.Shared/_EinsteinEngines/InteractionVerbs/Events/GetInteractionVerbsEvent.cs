// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.InteractionVerbs.Events;

/// <summary>
///     Raised directly on the user entity to get more interaction verbs it may allow.
///     While InteractionVerbsComponent defines which verbs may be performed on the entity,
///     This event allows to also define which verbs the entity itself may perform.<br/><br/>
///
///     Note that this is raised before IsAllowed checks are performed on any of the verbs.
/// </summary>
[ByRefEvent]
public sealed class GetInteractionVerbsEvent(List<ProtoId<InteractionVerbPrototype>> verbs)
{
    public List<ProtoId<InteractionVerbPrototype>> Verbs = verbs;

    public bool Add(ProtoId<InteractionVerbPrototype> verb)
    {
        if (Verbs.Contains(verb))
            return false;

        Verbs.Add(verb);
        return true;
    }
}
