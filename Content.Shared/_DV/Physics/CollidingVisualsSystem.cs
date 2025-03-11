using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Shared._DV.Physics;

public sealed class CollidingVisualsSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CollidingVisualsComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<CollidingVisualsComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnStartCollide(Entity<CollidingVisualsComponent> ent, ref StartCollideEvent args)
    {
        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.OtherEntity))
            return;

        // update active fixtures and state
        var state = ent.Comp.Default;
        foreach (var id in ent.Comp.Fixtures)
        {
            if (args.OurFixtureId == id)
            {
                ent.Comp.Active.Add(id);
                state = id;
                break;
            }
        }

        SetState(ent, state);
    }

    private void OnEndCollide(Entity<CollidingVisualsComponent> ent, ref EndCollideEvent args)
    {
        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.OtherEntity))
            return;

        ent.Comp.Active.Remove(args.OurFixtureId);

        // find the first state that is still active
        var state = ent.Comp.Default;
        foreach (var id in ent.Comp.Fixtures)
        {
            if (ent.Comp.Active.Contains(id))
            {
                state = id;
                break;
            }
        }

        SetState(ent, state);
    }

    public void SetState(EntityUid uid, string state)
    {
        _appearance.SetData(uid, CollidingVisuals.Fixture, state);
    }
}
