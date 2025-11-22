using Content.Client.Examine;
using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Power._FarHorizons.Power.Generation.FissionGenerator;

public sealed class NuclearReactorSystem : SharedNuclearReactorSystem
{
    private static readonly EntProtoId ArrowPrototype = "ReactorFlowArrow";

    public override void Initialize()
    {
        SubscribeLocalEvent<Shared.Power._FarHorizons.Power.Generation.FissionGenerator.NuclearReactorComponent, ClientExaminedEvent>(ReactorExamined);
    }

    private void ReactorExamined(EntityUid uid, Shared.Power._FarHorizons.Power.Generation.FissionGenerator.NuclearReactorComponent comp, ClientExaminedEvent args)
    {
        Spawn(ArrowPrototype, new EntityCoordinates(uid, 0, 0));
    }
}
