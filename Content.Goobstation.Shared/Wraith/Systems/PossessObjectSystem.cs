using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class PossessObjectSystem : EntitySystem
{
    [Dependency] private readonly ISerializationManager _seriMan = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly WraithPossessedSystem _possessed = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PossessObjectComponent, PossessObjectEvent>(OnPossess);
        SubscribeLocalEvent<PossessObjectEvent>(OnChangeComponents);
    }

    private void OnPossess(Entity<PossessObjectComponent> ent, ref PossessObjectEvent args)
    {

        if (!_mind.TryGetMind(args.Performer, out var mindId, out _))
            return;

        _popup.PopupClient(Loc.GetString("wraith-possess"), args.Target, args.Target);
        _audio.PlayPredicted(ent.Comp.PossessSound, args.Target, args.Target);

        // Make the object possessed
        var possession = EnsureComp<WraithPossessedComponent>(args.Target);
        var possessedObject = (args.Target, possession);

        _possessed.SetPossessionDuration(possessedObject, ent.Comp.PossessDuration);
        _possessed.StartPossession(possessedObject, args.Performer, mindId);

        args.Handled = true;
    }

    private void OnChangeComponents(PossessObjectEvent args)
    {
        var target = args.Target;

        args.Handled = true;

        RemoveComponents(target, args.ToRemove);
        AddComponents(target, args.ToAdd);
    }

    private void AddComponents(EntityUid target, ComponentRegistry comps)
    {
        foreach (var (name, data) in comps)
        {
            if (HasComp(target, data.Component.GetType()))
                continue;

            var component = (Component) Factory.GetComponent(name);
            var temp = (object) component;
            _seriMan.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(target, (Component) temp!);
        }
    }

    private void RemoveComponents(EntityUid target, HashSet<string> comps)
    {
        foreach (var toRemove in comps)
        {
            if (Factory.TryGetRegistration(toRemove, out var registration))
                RemComp(target, registration.Type);
        }
    }
}
