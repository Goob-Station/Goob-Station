using Content.Server.Store.Systems;
using Robust.Server.Player;
using Content.Server.Actions;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Content.Shared.FixedPoint;
using Content.Shared.MalfAi;
using Content.Shared.Actions;


namespace Content.Server.MalfAi;

public sealed partial class MalfAiSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiComponent, ComponentStartup>(OnStartup);
        SubscribeAbilities();
    }
    private void OnStartup(EntityUid uid, MalfAiComponent comp, ref ComponentStartup args)
    {
        foreach (var actionId in comp.BaseMalfAiActions)
            _actions.AddAction(uid, actionId);
    }
};
