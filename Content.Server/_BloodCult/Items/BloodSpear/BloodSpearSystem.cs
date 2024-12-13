using Content.Server.Actions;
using Content.Server.Hands.Systems;
using Content.Server.Stunnable;
using Content.Shared.Humanoid;
using Content.Shared.Item;
using Content.Shared.Projectiles;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Server.Audio;

namespace Content.Server.WhiteDream.BloodCult.Items.BloodSpear;

public sealed class BloodSpearSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodSpearComponent, EmbedEvent>(OnEmbed);

        SubscribeLocalEvent<BloodSpearComponent, GettingPickedUpAttemptEvent>(OnPickedUp);
        SubscribeLocalEvent<BloodCultistComponent, BloodSpearRecalledEvent>(OnSpearRecalled);

        SubscribeLocalEvent<BloodSpearComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentShutdown(Entity<BloodSpearComponent> spear, ref ComponentShutdown args)
    {
        if (!spear.Comp.RecallAction.HasValue)
            return;

        _actions.RemoveAction(spear.Comp.RecallAction);
    }

    private void OnEmbed(Entity<BloodSpearComponent> spear, ref EmbedEvent args)
    {
        if (!HasComp<HumanoidAppearanceComponent>(args.Embedded))
            return;

        _stun.TryParalyze(args.Embedded, spear.Comp.ParalyzeTime, true);
        QueueDel(spear);
    }

    private void OnPickedUp(Entity<BloodSpearComponent> spear, ref GettingPickedUpAttemptEvent args)
    {
        if (args.Cancelled || spear.Comp.Master.HasValue ||
            !TryComp(args.User, out BloodCultistComponent? bloodCultist))
            return;

        spear.Comp.Master = args.User;

        var action = _actions.AddAction(args.User, spear.Comp.RecallActionId);
        spear.Comp.RecallAction = action;
        bloodCultist.BloodSpear = spear;
    }

    private void OnSpearRecalled(Entity<BloodCultistComponent> cultist, ref BloodSpearRecalledEvent args)
    {
        if (args.Handled)
            return;

        var spearUid = cultist.Comp.BloodSpear;
        if (!spearUid.HasValue || !TryComp(spearUid, out BloodSpearComponent? spear))
            return;

        _hands.TryForcePickupAnyHand(cultist, spearUid.Value);
        _audio.PlayPvs(spear.RecallAudio, spearUid.Value);
        args.Handled = true;
    }

    public void DetachSpearFromMaster(Entity<BloodCultistComponent> cultist)
    {
        if (cultist.Comp.BloodSpear is not { } spearUid || !TryComp(spearUid, out BloodSpearComponent? spear))
            return;

        _actions.RemoveAction(spear.RecallAction);
        spear.RecallAction = null;
        spear.Master = null;
        cultist.Comp.BloodSpear = null;
    }
}
