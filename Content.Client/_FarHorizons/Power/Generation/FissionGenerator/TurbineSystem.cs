using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Robust.Client.GameObjects;
using Content.Shared.Repairable;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Client.Popups;
using Content.Client.Examine;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed class TurbineSystem : SharedTurbineSystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

    private static readonly EntProtoId ArrowPrototype = "TurbineFlowArrow";

    public override void Initialize()
    {
        SubscribeLocalEvent<TurbineComponent, ClientExaminedEvent>(ReactorExamined);
    }

    protected override void UpdateUi(Entity<TurbineComponent> entity)
    {
        if (_userInterfaceSystem.TryGetOpenUi(entity.Owner, TurbineUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
    protected override void OnRepairTurbineFinished(Entity<TurbineComponent> ent, ref RepairFinishedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp(ent.Owner, out TurbineComponent? comp))
            return;

        _popupSystem.PopupClient(Loc.GetString("turbine-repair", ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
    }

    private void ReactorExamined(EntityUid uid, TurbineComponent comp, ClientExaminedEvent args)
    {
        Spawn(ArrowPrototype, new EntityCoordinates(uid, 0, 0));
    }
}
