using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Client.Examine;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed class NuclearReactorSystem : SharedNuclearReactorSystem
{
    private static readonly EntProtoId ArrowPrototype = "ReactorFlowArrow";

    public override void Initialize()
    {
        SubscribeLocalEvent<NuclearReactorComponent, ClientExaminedEvent>(ReactorExamined);
    }

    private void ReactorExamined(EntityUid uid, NuclearReactorComponent comp, ClientExaminedEvent args)
    {
        Spawn(ArrowPrototype, new EntityCoordinates(uid,0, 0));
     } 
}