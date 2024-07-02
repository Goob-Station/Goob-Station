using Content.Client.Alerts;
using Content.Shared.Changeling;

namespace Content.Client.Changeling;

public sealed partial class ChangelingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void OnUpdateAlert(EntityUid uid, ChangelingComponent comp, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.AlertKey.AlertType != "")
            return;
    }
}
