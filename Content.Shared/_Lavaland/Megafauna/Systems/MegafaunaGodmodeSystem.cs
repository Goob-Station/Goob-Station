using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared.Damage;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed class MegafaunaGodmodeSystem : EntitySystem
{

    private EntityQuery<MegafaunaAiComponent> _megafaunaQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaGodmodeComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
        _megafaunaQuery = GetEntityQuery<MegafaunaAiComponent>();
    }

    private void OnBeforeDamageChanged(Entity<MegafaunaGodmodeComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Origin == null
            || _megafaunaQuery.TryComp(ent, out var ai)
            && !ai.Active)
            args.Cancelled = true;
    }
}
