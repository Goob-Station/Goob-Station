using Content.Shared.Body.Components; // Goobstation
using Content.Shared.Body.Part; // Goobstation
using Content.Shared._White.Standing;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server.Standing;

public sealed class LayingDownSystem : SharedLayingDownSystem
{
    [Dependency] private readonly INetConfigurationManager _cfg = default!;
    [Dependency] private readonly EntityManager _entMan = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CheckAutoGetUpEvent>(OnCheckAutoGetUp);
    }

    private void OnCheckAutoGetUp(CheckAutoGetUpEvent ev, EntitySessionEventArgs args)
    {
        var uid = GetEntity(ev.User);

        if (!TryComp(uid, out LayingDownComponent? layingDown))
            return;

        // Goobstation start
        bool fullyParalyzed = false;

        if (_entMan.TryGetComponent<BodyComponent>(uid, out var body))
        {
            foreach (var legEntity in body.LegEntities)
            {
                if (_entMan.TryGetComponent<BodyPartComponent>(legEntity, out var partCmp))
                {
                    if (partCmp.Enabled != true)
                    {
                        fullyParalyzed = true;
                        continue;
                    } else if (partCmp.Enabled == true)
                    {
                        fullyParalyzed = false;
                        break;
                    }
                }
            }
        }

        if (fullyParalyzed)
        {
            layingDown.AutoGetUp = false;
            Dirty(uid, layingDown);
            return;
        }
        // Goobstation end

        layingDown.AutoGetUp = _cfg.GetClientCVar(args.SenderSession.Channel, CCVars.AutoGetUp);
        Dirty(uid, layingDown);
    }
}
