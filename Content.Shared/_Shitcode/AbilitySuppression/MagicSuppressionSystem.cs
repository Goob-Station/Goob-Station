using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Popups;

namespace Content.Shared.AbilitySuppression;

public sealed partial class MagicSuppressionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicSuppressionComponent, InventoryRelayedEvent<CheckMagicSuppressionEvent>>(OnCheckSuppression);
        SubscribeLocalEvent<MagicSuppressionComponent, ExaminedEvent>(OnMagicSuppressionExamine);

    }

    private void OnCheckSuppression(Entity<MagicSuppressionComponent> ent, ref InventoryRelayedEvent<CheckMagicSuppressionEvent> args)
    {
        args.Args.Blocker = ent;
        args.Args.Cancelled = true;
    }

    private void OnMagicSuppressionExamine(Entity<MagicSuppressionComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("suppression-item-examine"));
    }
}
