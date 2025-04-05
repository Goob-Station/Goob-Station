using Content.Goobstation.Server.Wizard.Components;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed class StruckByLightningSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<Common.Wizard.Components.StruckByLightningComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Lifetime -= frameTime;

            if (comp.Lifetime > 0)
                continue;

            RemCompDeferred(uid, comp);
        }
    }
}
