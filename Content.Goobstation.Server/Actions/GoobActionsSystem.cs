using Content.Goobstation.Shared.Actions;

namespace Content.Goobstation.Server.Actions;
public sealed partial class GoobActionsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<ActionRemovedFromUIControllerMessage>(OnActionRemovedFromUIController);
    }

    private void OnActionRemovedFromUIController(ActionRemovedFromUIControllerMessage ev)
    {
        RaiseLocalEvent(GetEntity(ev.Action), new ActionRemovedFromUIControllerEvent());
    }
}
