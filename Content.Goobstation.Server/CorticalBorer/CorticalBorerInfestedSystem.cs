using Content.Goobstation.Shared.CorticalBorer.Components;
using Content.Shared.Body.Part;
using Content.Shared.Examine;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.CorticalBorer;

public sealed class CorticalBorerInfestedSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CorticalBorerSystem _borer = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<CorticalBorerInfestedComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<CorticalBorerInfestedComponent, ExaminedEvent>(OnExaminedInfested);

        SubscribeLocalEvent<CorticalBorerInfestedComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);
        SubscribeLocalEvent<CorticalBorerInfestedComponent, MobStateChangedEvent>(OnStateChange);
        SubscribeLocalEvent<CorticalBorerInfestedComponent, MindRemovedMessage>(OnMindRemoved);
    }

    private void OnInit(Entity<CorticalBorerInfestedComponent> infested, ref MapInitEvent args)
    {
        infested.Comp.ControlContainer = _container.EnsureContainer<Container>(infested, "ControlContainer");
        infested.Comp.InfestationContainer = _container.EnsureContainer<Container>(infested, "InfestationContainer");
    }

    private void OnExaminedInfested(Entity<CorticalBorerInfestedComponent> infected, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!infected.Comp.Borer.Comp.ControlingHost)
            return;

        args.PushMarkup(Loc.GetString("cortical-borer-infested-examine"));

        if (args.Examined != args.Examiner)
            return;

        if (infected.Comp.ControlTimeEnd is { } cte)
        {
            var timeRemaining = Math.Floor((cte - _timing.CurTime).TotalSeconds);
            args.PushMarkup(Loc.GetString("infested-control-examined", ("timeremaining", timeRemaining)));
        }

        args.PushMarkup(Loc.GetString("cortical-borer-self-examine", ("chempoints", infected.Comp.Borer.Comp.ChemicalPoints)));
    }

    private void OnStateChange(Entity<CorticalBorerInfestedComponent> infected, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if(infected.Comp.Borer.Comp.ControlingHost)
            _borer.EndControl(infected);
    }

    private void OnBodyPartRemoved(Entity<CorticalBorerInfestedComponent> infected, ref BodyPartRemovedEvent args)
    {
        if (!TryComp<BodyPartComponent>(args.Part, out var part) ||
            part.PartType != BodyPartType.Head)
            return;

        _borer.EndControl(infected);
        _borer.TryEjectBorer(infected.Comp.Borer);
    }

    private void OnMindRemoved(Entity<CorticalBorerInfestedComponent> infected, ref MindRemovedMessage args)
    {
        if (!infected.Comp.Borer.Comp.ControlingHost)
            return;

        _borer.EndControl(infected);
        _borer.TryEjectBorer(infected.Comp.Borer);
    }
}
