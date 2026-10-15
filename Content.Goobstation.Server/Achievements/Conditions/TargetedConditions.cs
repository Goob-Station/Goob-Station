using Content.Goobstation.Common.Kitchen;
using Content.Goobstation.Shared.Bible;
using Content.Server.Access.Systems;
using Content.Server.GameTicking;
using Content.Server.Holiday.Christmas;
using Content.Server.Xenoarchaeology.Artifact.XAE.Components;
using Content.Shared.Access.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Kitchen;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition;
using Content.Shared.Polymorph;
using Content.Shared.Xenoarchaeology.Artifact.Components;
using Content.Shared.Xenoarchaeology.Artifact;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Achievements.Conditions;

public abstract partial class PrototypeFilteredCondition : AchievementCondition
{
    [DataField]
    public List<EntProtoId>? Prototypes;
}

public sealed partial class ButcherCondition : PrototypeFilteredCondition;

public sealed partial class EatenCondition : PrototypeFilteredCondition;

public sealed partial class BibleSmiteCondition : PrototypeFilteredCondition;

public sealed partial class PolymorphedIntoCondition : PrototypeFilteredCondition;

public sealed partial class GiftOpenedCondition : PrototypeFilteredCondition;

public sealed partial class MicrowaveIdCondition : AchievementCondition;

public sealed partial class ArtifactExplosionCondition : AchievementCondition
{
    [DataField]
    public TimeSpan Within = TimeSpan.FromMinutes(7);
}

public abstract partial class PrototypeFilteredConditionSystem<T> : AchievementConditionSystem<T> where T : PrototypeFilteredCondition
{
    protected void ProgressMatching(EntityUid player, EntProtoId? target)
    {
        foreach (var (proto, condition) in Entries)
        {
            if (condition.Prototypes is { } prototypes && (target is not { } id || !prototypes.Contains(id)))
                continue;

            Achievements.AddProgress(player, proto);
        }
    }

    protected EntProtoId? GetProto(EntityUid uid)
    {
        return Prototype(uid) is { } proto ? new EntProtoId(proto.ID) : (EntProtoId?) null;
    }
}

public sealed partial class ButcherConditionSystem : PrototypeFilteredConditionSystem<ButcherCondition>
{
    [Dependency] private SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, BeforeBeingButcheredEvent>(OnButchered);
    }

    private void OnButchered(Entity<MobStateComponent> ent, ref BeforeBeingButcheredEvent args)
    {
        if (args.Weapon is not { } weapon)
            return;

        var butcher = HasComp<ActorComponent>(weapon)
            ? weapon
            : _container.TryGetContainingContainer((weapon, null, null), out var container) ? container.Owner : weapon;

        ProgressMatching(butcher, GetProto(ent));
    }
}

public sealed partial class EatenConditionSystem : PrototypeFilteredConditionSystem<EatenCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, FullyEatenEvent>(OnEaten);
    }

    private void OnEaten(Entity<MobStateComponent> ent, ref FullyEatenEvent args)
    {
        ProgressMatching(args.User, GetProto(ent));
    }
}

public sealed partial class BibleSmiteConditionSystem : PrototypeFilteredConditionSystem<BibleSmiteCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, BibleSmiteUsed>(OnSmite);
    }

    private void OnSmite(Entity<MobStateComponent> ent, ref BibleSmiteUsed args)
    {
        ProgressMatching(args.User, GetProto(ent));
    }
}

public sealed partial class PolymorphedIntoConditionSystem : PrototypeFilteredConditionSystem<PolymorphedIntoCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnPolymorphed(Entity<ActorComponent> ent, ref PolymorphedEvent args)
    {
        if (args.IsRevert || ent.Owner != args.NewEntity)
            return;

        ProgressMatching(ent, GetProto(ent));
    }
}

public sealed partial class GiftOpenedConditionSystem : PrototypeFilteredConditionSystem<GiftOpenedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemComponent, UseInHandEvent>(OnUseInHand, before: [typeof(RandomGiftSystem)]);
    }

    private void OnUseInHand(Entity<ItemComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || !TryComp<RandomGiftComponent>(ent, out var gift) || gift.SelectedEntity is not { } selected)
            return;

        ProgressMatching(args.User, new EntProtoId(selected));
    }
}

public sealed partial class MicrowaveIdConditionSystem : AchievementConditionSystem<MicrowaveIdCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AccessComponent, BeingMicrowavedEvent>(OnMicrowaved, after: [typeof(IdCardSystem)]);
    }

    private void OnMicrowaved(Entity<AccessComponent> ent, ref BeingMicrowavedEvent args)
    {
        if (args.User is { } user && HasComp<IdCardComponent>(ent))
            ProgressAll(user);
    }
}

public sealed partial class ArtifactExplosionConditionSystem : AchievementConditionSystem<ArtifactExplosionCondition>
{
    [Dependency] private GameTicker _ticker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoArtifactNodeComponent, XenoArtifactNodeActivatedEvent>(OnActivated);
    }

    private void OnActivated(Entity<XenoArtifactNodeComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        if (args.User is not { } user || !HasComp<XAETriggerExplosivesComponent>(ent))
            return;

        var elapsed = _ticker.RoundDuration();
        foreach (var (proto, condition) in Entries)
        {
            if (elapsed <= condition.Within)
                Achievements.AddProgress(user, proto);
        }
    }
}
