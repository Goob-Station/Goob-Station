// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Shared.Administration
{
    /// <summary>
    ///     Specifies that a command can be executed by any player.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    [MeansImplicitUse]
    public sealed class AnyCommandAttribute : Attribute
    {

    }
}