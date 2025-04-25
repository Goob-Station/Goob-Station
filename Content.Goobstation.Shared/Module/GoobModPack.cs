using Content.Shared.Module;

namespace Content.Goobstation.Shared.Module;

public sealed class GoobModPack : ModulePack
{
    public override string PackName => "Goobstation"; // would call it goobmod but will make it clearer what your supposed to be rebuilding

    public override IReadOnlySet<RequiredAssembly> RequiredAssemblies { get; } = new HashSet<RequiredAssembly>
    {
        RequiredAssembly.ForClient("Content.Goobstation.Client"),
        RequiredAssembly.ForServer("Content.Goobstation.Server"),
        RequiredAssembly.ForBoth("Content.Goobstation.Common"),
    };
}
