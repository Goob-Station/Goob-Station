// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs.Requirements;

/// <summary>
///     A requirement that combines multiple other requirements.
/// </summawry>
[Serializable, NetSerializable]
public sealed partial class ComplexRequirement : InteractionRequirement
{
    [DataField]
    public List<InteractionRequirement> Requirements = new();

    /// <summary>
    ///     If true, all requirements must pass (boolean and). Otherwise, at least one must pass (boolean or).
    /// </summary>
    [DataField]
    public bool RequireAll = true;

    public override bool IsMet(InteractionArgs args, InteractionVerbPrototype proto, InteractionAction.VerbDependencies deps)
    {
        return RequireAll
            ? Requirements.All(r => r.IsMet(args, proto, deps))
            : Requirements.Any(r => r.IsMet(args, proto, deps));
    }
}
