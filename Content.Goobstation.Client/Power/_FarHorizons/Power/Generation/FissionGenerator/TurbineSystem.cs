using Content.Client.Examine;
using Content.Client.Popups;
using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Repairable;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Power._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/turbine.dm

public sealed class TurbineSystem : SharedTurbineSystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

    private static readonly EntProtoId ArrowPrototype = "TurbineFlowArrow";

    public override void Initialize()
    {
        SubscribeLocalEvent<Shared.Power._FarHorizons.Power.Generation.FissionGenerator.TurbineComponent, ClientExaminedEvent>(ReactorExamined);
    }

    protected override void UpdateUI(EntityUid uid, Shared.Power._FarHorizons.Power.Generation.FissionGenerator.TurbineComponent turbine)
    {
        if (_userInterfaceSystem.TryGetOpenUi(uid, TurbineUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
    protected override void OnRepairTurbineFinished(Entity<Shared.Power._FarHorizons.Power.Generation.FissionGenerator.TurbineComponent> ent, ref RepairFinishedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp(ent.Owner, out Shared.Power._FarHorizons.Power.Generation.FissionGenerator.TurbineComponent? comp))
            return;

        _popupSystem.PopupClient(Loc.GetString("turbine-repair", ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
    }

    private void ReactorExamined(EntityUid uid, Shared.Power._FarHorizons.Power.Generation.FissionGenerator.TurbineComponent comp, ClientExaminedEvent args)
    {
        Spawn(ArrowPrototype, new EntityCoordinates(uid, 0, 0));
    }
}
