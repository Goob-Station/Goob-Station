using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class AmputatePart : EntityEffectBase<AmputatePart>
{
    [DataField(required: true)]
    public BodyPartType PartType;

    [DataField]
    public int Max = 1;
};

public sealed partial class AmputatePartEntityEffectSystem : EntityEffectSystem<BodyComponent, AmputatePart>
{
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private WoundSystem _wound = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<AmputatePart> args)
    {
        var parts = _body.GetBodyChildrenOfType(ent, args.Effect.PartType).ToList();

        for (var i = 0; i < args.Effect.Max; i++)
        {
            var part = parts[i].Id;

            if (!TryComp(part, out WoundableComponent? woundableComp))
                continue;

            _wound.AmputateWoundable(ent, part, woundableComp);
        }
    }
}
