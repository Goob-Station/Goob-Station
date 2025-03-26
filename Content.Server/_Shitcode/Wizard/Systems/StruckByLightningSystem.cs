using Content.Server._Goobstation.Wizard.Components;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class StruckByLightningSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StruckByLightningComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Lifetime -= frameTime;

            if (comp.Lifetime > 0)
                continue;

            RemCompDeferred(uid, comp);
        }
    }
}
