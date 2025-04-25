namespace Content.Shared.Module;

public record struct RequiredAssembly(string AssemblyName, bool Server = true, bool Client = false)
{
    public static RequiredAssembly ForServer(string assembly) => new(assembly, Server: true, Client: false);

    public static RequiredAssembly ForClient(string assembly) => new(assembly, Server: false, Client: true);

    public static RequiredAssembly ForBoth(string assembly) => new(assembly, Server: true, Client: true);
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
