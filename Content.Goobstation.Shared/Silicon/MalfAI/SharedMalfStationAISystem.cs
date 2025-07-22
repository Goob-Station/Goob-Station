using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Silicon.MalfAI;

public sealed class SharedMalfStationAISystem : EntitySystem
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
            Act = () => StartHackDoAfter(hackable, malfEntity),
            Text = Loc.GetString("malf-ai-hack-verb-text"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/open.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    private void StartHackDoAfter(Entity<MalfStationAIHackableComponent> hackable, EntityUid malfEntity)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager, malfEntity, 10, new HackDoAfterEvent(), malfEntity, hackable);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnHackDoAfterComplete(Entity<MalfStationAIComponent> ent, ref HackDoAfterEvent args)
    {
        if (!args.Used.HasValue)
            return;

        var ev = new OnHackedEvent(args.Used.Value, ent.Owner);

        RaiseLocalEvent(ref ev);
    }
}