using Content.Shared.GameTicking;
using Content.Shared.Administration.Components;
using Content.Server.Administration.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Goida;

public sealed class JohnGoidaSystem : EntitySystem
{
    /// <summary>
    /// Fuck xeve
    /// </summary>
    public const string Xeve = "Xeve Byrd";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (!TryComp<MetaDataComponent>(args.Mob, out var meta))
            return;

        var name = meta.EntityName;
        if (string.Equals(name, Xeve, StringComparison.OrdinalIgnoreCase))
        {
            EnsureComp<KillSignComponent>(args.Mob);
        }
    }
}
