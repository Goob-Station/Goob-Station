using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Goobstation.Shared.Silicon.MalfAI.Events;
using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Silicon.MalfAI;

public abstract class SharedMalfStationAISystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MalfStationAIHackableComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<MalfStationAIComponent, HackDoAfterEvent>(OnHackDoAfterComplete);
    }

    private void OnGetVerbs(Entity<MalfStationAIHackableComponent> hackable, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var malfEntity = args.User;

        if (!TryComp<MalfStationAIComponent>(malfEntity, out var malf))
            return;

        if (hackable.Comp.Hacked)
            return;

        var verb = new AlternativeVerb
        {
            Priority = 1,
            Act = () => StartHackDoAfter(hackable, new Entity<MalfStationAIComponent>(malfEntity, malf)),
            Text = Loc.GetString("malf-ai-hack-verb-text"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/open.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    private void StartHackDoAfter(Entity<MalfStationAIHackableComponent> hackable, Entity<MalfStationAIComponent> malfEntity)
    {
        if (_doAfter.IsRunning(malfEntity.Comp.HackDoAfterID))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, malfEntity, hackable.Comp.SecondsToHack, new HackDoAfterEvent(), malfEntity, hackable);

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out var id))
            return;

        malfEntity.Comp.HackDoAfterID = id;
    }

    private void OnHackDoAfterComplete(Entity<MalfStationAIComponent> ent, ref HackDoAfterEvent args)
    {
        if (!TryComp<MalfStationAIHackableComponent>(args.Target, out var hackable))
            return;

        hackable.Hacked = true;

        var ev = new OnHackedEvent(ent);

        RaiseLocalEvent(args.Target.Value, ref ev);
    }
}