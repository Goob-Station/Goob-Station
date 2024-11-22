using System.Threading;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Jittering;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects;

[UsedImplicitly]
public sealed partial class PurifyEvil : ReagentEffect
{
    [DataField]
    public float Amplitude = 10.0f;

    [DataField]
    public float Frequency = 4.0f;

    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(30.0f);

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-purify-evil");
    }

    public override void Effect(ReagentEffectArgs args)
    {
        var entityManager = args.EntityManager;
        var uid = args.SolutionEntity;
        if (!entityManager.TryGetComponent(uid, out BloodCultistComponent? bloodCultist) ||
            bloodCultist.DeconvertToken is not null)
        {
            return;
        }

        entityManager.System<SharedJitteringSystem>().DoJitter(uid, Time, true, Amplitude, Frequency);

        bloodCultist.DeconvertToken = new CancellationTokenSource();
        Robust.Shared.Timing.Timer.Spawn(Time, () => DeconvertCultist(uid, entityManager),
            bloodCultist.DeconvertToken.Token);
    }

    private void DeconvertCultist(EntityUid uid, IEntityManager entityManager)
    {
        if (entityManager.HasComponent<BloodCultistComponent>(uid))
            entityManager.RemoveComponent<BloodCultistComponent>(uid);
    }
}
