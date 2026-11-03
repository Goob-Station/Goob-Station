using Content.Goobstation.Shared.Blob.Components;
using Content.Shared.Tools;
using Content.Shared.Tools.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class BlobPryCondition : AchievementCondition;

public sealed partial class BlobPryConditionSystem : AchievementConditionSystem<BlobPryCondition>
{
    [Dependency] private SharedToolSystem _tool = default!;

    private static readonly ProtoId<ToolQualityPrototype> PryingQuality = "Prying";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobTileComponent, AttackedEvent>(OnAttacked);
    }

    private void OnAttacked(Entity<BlobTileComponent> ent, ref AttackedEvent args)
    {
        if (args.Used != args.User && _tool.HasQuality(args.Used, PryingQuality))
            ProgressAll(args.User);
    }
}
