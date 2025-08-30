using Content.Shared.Body.Systems;
using Content.Shared.PowerCell;
using Content.Shared._Shitmed.Body.Organ;

namespace Content.Goobstation.Shared.Augments;

public abstract class SharedAugmentPowerCellSystem : EntitySystem
{
    [Dependency] protected readonly AugmentSystem Augment = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] protected readonly SharedPowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        _drawQuery = GetEntityQuery<PowerCellDrawComponent>();

        SubscribeLocalEvent<AugmentPowerCellSlotComponent, OrganEnableChangedEvent>(OnEnableChanged);
        SubscribeLocalEvent<AugmentPowerCellSlotComponent, PowerCellSlotEmptyEvent>(OnCellEmpty);
    }

    private EntityQuery<PowerCellDrawComponent> _drawQuery;

    private void OnEnableChanged(Entity<AugmentPowerCellSlotComponent> ent, ref OrganEnableChangedEvent args)
    {
        if (!_drawQuery.TryComp(ent, out var draw))
            return;

        UpdateDrawRate((ent, draw));

        _powerCell.SetDrawEnabled(ent.Owner, args.Enabled);
        if (Augment.GetBody(ent) is not {} body)
            return;

        if (args.Enabled && _powerCell.HasDrawCharge(ent.Owner, draw))
        {
            var ev = new AugmentGainedPowerEvent(body);
            Augment.RelayEvent(body, ref ev);
        }
        else
        {
            var ev = new AugmentLostPowerEvent(body);
            Augment.RelayEvent(body, ref ev);
        }
    }

    private void OnCellEmpty(Entity<AugmentPowerCellSlotComponent> ent, ref PowerCellSlotEmptyEvent args)
    {
        if (Augment.GetBody(ent) is not {} body)
            return;

        var ev = new AugmentLostPowerEvent(body);
        Augment.RelayEvent(body, ref ev);

        // stop drawing if it loses power
        UpdateDrawRate(ent.Owner);
    }

    public float GetBodyDraw(EntityUid body)
    {
        var ev = new GetAugmentsPowerDrawEvent(body);
        Augment.RelayEvent(body, ref ev);
        return ev.TotalDraw;
    }

    /// <summary>
    /// Update the draw rate for a power cell slot augment.
    /// </summary>
    public void UpdateDrawRate(Entity<PowerCellDrawComponent?> ent)
    {
        if (!_drawQuery.Resolve(ent, ref ent.Comp))
            return;

        var rate = Augment.GetBody(ent) is {} body
            ? GetBodyDraw(body)
            : 0f;
        if (ent.Comp.DrawRate == rate)
            return;

        ent.Comp.DrawRate = rate;
        Dirty(ent, ent.Comp);
    }

    /// <summary>
    /// Get a body's power cell slot augment, or null if it has none.
    /// </summary>
    public Entity<AugmentPowerCellSlotComponent>? GetBodyAugment(EntityUid body)
    {
        foreach (var augment in _body.GetBodyOrganEntityComps<AugmentPowerCellSlotComponent>(body))
        {
            return (augment.Owner, augment.Comp1);
        }

        return null;
    }
}
