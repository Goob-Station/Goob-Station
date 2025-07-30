using Content.Server.Psionics;

namespace Content.Server.Traits.Assorted;
public sealed partial class PotentiaModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PotentiaModifierComponent, OnRollPsionicsEvent>(OnRollPsionics);
    }

    private void OnRollPsionics(EntityUid uid, PotentiaModifierComponent component, ref OnRollPsionicsEvent args)
    {
        if (uid != args.Roller)
            return;

        args.BaselineChance = args.BaselineChance * component.PotentiaMultiplier + component.PotentiaFlatModifier;
    }
}
