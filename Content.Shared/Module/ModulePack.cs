// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Module;

public record struct RequiredAssembly(string AssemblyName, bool IsServer = true, bool IsClient = false)
{
    public static RequiredAssembly Server(string assembly) => new(assembly, IsServer: true, IsClient: false);

    public static RequiredAssembly Client(string assembly) => new(assembly, IsServer: false, IsClient: true);

    public static RequiredAssembly Shared(string assembly) => new(assembly, IsServer: true, IsClient: true);
}

public abstract class ModulePack
{
    /// <summary>
    /// A readable name to identify the module eg. Goobmod.
    /// </summary>
    public abstract string PackName { get; }

    /// <summary>
    /// List of required assembly names (not file paths).
    /// </summary>
    public abstract IReadOnlySet<RequiredAssembly> RequiredAssemblies { get; }
}
