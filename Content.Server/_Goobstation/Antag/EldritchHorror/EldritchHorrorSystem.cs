using Content.Shared.Actions;

namespace Content.Server._Goobstation.Antag.EldritchHorror;
using Content.Shared._Goobstation.EldritchHorror.EldritchHorrorEvents;

public sealed partial class EldritchHorrorSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EldritchHorrorComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<EldritchHorrorComponent, HorrorSpawnProphetActionEvent>(OnRiseProphets);
    }

    private void OnInit(EntityUid uid, EldritchHorrorComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.SpawnProphetsActionEntity, component.SpawnProphetsAction);
    }

    private void OnRiseProphets(EntityUid uid, EldritchHorrorComponent component, HorrorSpawnProphetActionEvent args)
    {
        if (args.Handled)
            return;

        var xform = Transform(uid);
        for (int i = 0; i < component.ProphetAmount; i++)
        {
            var ent = Spawn(component.ProphetProtoId, xform.Coordinates);
        }

        args.Handled = true;
    }
}
