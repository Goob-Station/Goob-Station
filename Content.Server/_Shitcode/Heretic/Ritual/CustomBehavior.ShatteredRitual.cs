using Content.Server.Polymorph.Systems;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Polymorph;
using Content.Shared.Speech.Muting;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualShatteredBehavior : RitualSacrificeBehavior
{
    [DataField]
    public ProtoId<PolymorphPrototype> Polymorph = "ShatteredRisen";

    public override void Finalize(RitualData args)
    {
        if (args is { Limit: > 0, Limited: not null } && args.Limited.Count >= args.Limit)
            return;

        for (var i = 0; i < Math.Min(uids.Count, Max); i++)
        {
            var uid = uids[i];

            var result = args.EntityManager.System<PolymorphSystem>().PolymorphEntity(uid, Polymorph);

            if (result == null)
                continue;

            var ghoul = new GhoulComponent
            {
                TotalHealth = 250,
                BoundHeretic = args.Performer,
                ChangeHumanoidAppearance = false,
            };

            args.EntityManager.AddComponent(result.Value, ghoul, overwrite: true);

            if (args.Limited == null)
                continue;

            args.Limited.Add(result.Value);

            if (args.Limit > 0 && args.Limited.Count >= args.Limit)
                break;
        }
    }
}
