using Content.Server._Goobstation.Wizard.Systems;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Body;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.Xenomorphs.Infection;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Ghost;

public sealed partial class GhostSystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly GhostVisibilitySystem _ghostVisibility = default!;


    private static readonly ProtoId<DamageTypePrototype> IonDamageType = "Ion";

    private bool GoobIsSuicideAllowedIC(EntityUid playerEntity)
    {
        return HasComp<XenomorphPreventSuicideComponent>(playerEntity);
    }

    private ProtoId<DamageTypePrototype> GoobGetSuicideDamageType(EntityUid playerEntity)
    {
        return HasComp<SiliconComponent>(playerEntity)
            ? IonDamageType
            : AsphyxiationDamageType;
    }

    private TargetBodyPart? GoobGetShitmedSuicideTarget(EntityUid playerEntity)
    {
        if (TryComp<BodyComponent>(playerEntity, out var body)
            && body.BodyType == BodyType.Complex
            && body.RootContainer.ContainedEntities.FirstOrNull() is { } root)
            return TargetBodyPart.All;
        return null;
    }
}