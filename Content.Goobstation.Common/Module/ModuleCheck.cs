// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Module;

public abstract class ModuleCheck
{
    /// <summary>
    /// A human‐readable name for the pack (e.g. "Goobmod").
    /// </summary>
    public abstract string PackName { get; }
}
