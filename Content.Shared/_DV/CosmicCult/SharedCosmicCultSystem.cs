using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Antag;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Content.Shared._DV.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult;

public abstract class SharedCosmicCultSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, ComponentGetStateAttemptEvent>(OnCosmicCultCompGetStateAttempt);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentGetStateAttemptEvent>(OnCosmicCultCompGetStateAttempt);
        SubscribeLocalEvent<CosmicCultComponent, ComponentStartup>(DirtyCosmicCultComps);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentStartup>(DirtyCosmicCultComps);

        SubscribeLocalEvent<CosmicTransmutableComponent, GetVerbsEvent<ExamineVerb>>(OnTransmutableExamined);
        SubscribeLocalEvent<CosmicCultExamineComponent, ExaminedEvent>(OnCosmicCultExamined);
        SubscribeLocalEvent<CosmicSubtleMarkComponent, ExaminedEvent>(OnSubtleMarkExamined);
    }

    private void OnTransmutableExamined(Entity<CosmicTransmutableComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (ent.Comp.TransmutesTo == "" || ent.Comp.RequiredGlyphType == "") return;
        if (!EntityIsCultist(args.User)) //non-cultists don't need to know this anyway
            return;
        var result = _proto.Index(ent.Comp.TransmutesTo).Name;
        var glyph = _proto.Index(ent.Comp.RequiredGlyphType).Name;
        var text = Loc.GetString("cosmic-examine-transmutable", ("result", result), ("glyph", glyph));
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(text);
        _examine.AddHoverExamineVerb(args,
            ent.Comp,
            Loc.GetString("cosmic-examine-transmutable-verb-text"),
            msg.ToMarkup(),
            "/Textures/_DV/CosmicCult/Interface/transmute_inspect.png");
    }

    private void OnCosmicCultExamined(Entity<CosmicCultExamineComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(EntitySeesCult(args.Examiner) ? ent.Comp.CultistText : ent.Comp.OthersText));
    }

    private void OnSubtleMarkExamined(Entity<CosmicSubtleMarkComponent> ent, ref ExaminedEvent args)
    {
        var ev = new SeeIdentityAttemptEvent();
        RaiseLocalEvent(ent, ev);
        if (ev.TotalCoverage.HasFlag(IdentityBlockerCoverage.EYES)) return;

        args.PushMarkup(Loc.GetString(ent.Comp.ExamineText));
    }

    public bool EntityIsCultist(EntityUid user)
    {
        if (!_mind.TryGetMind(user, out var mind, out _))
            return false;

        return HasComp<CosmicCultComponent>(user) || _role.MindHasRole<CosmicCultRoleComponent>(mind);
    }

    public bool EntitySeesCult(EntityUid user)
    {
        return EntityIsCultist(user) || HasComp<GhostComponent>(user);
    }

    /// <summary>
    /// Determines if a Cosmic Cult Lead component should be sent to the client.
    /// </summary>
    private void OnCosmicCultCompGetStateAttempt(EntityUid uid, CosmicCultLeadComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// Determines if a Cosmic Cultist component should be sent to the client.
    /// </summary>
    private void OnCosmicCultCompGetStateAttempt(EntityUid uid, CosmicCultComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// The criteria that determine whether a Cult Member component should be sent to a client.
    /// </summary>
    /// <param name="player">The Player the component will be sent to.</param>
    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not { } uid)
            return true;

        if (EntitySeesCult(uid)
            || HasComp<CosmicCultLeadComponent>(uid))
            return true;

        return HasComp<ShowAntagIconsComponent>(uid);
    }

    /// <summary>
    /// Dirties all the Cult components so they are sent to clients.
    ///
    /// We need to do this because if a Cult component was not earlier sent to a client and for example the client
    /// becomes a Cult then we need to send all the components to it. To my knowledge there is no way to do this on a
    /// per client basis so we are just dirtying all the components.
    /// </summary>
    private void DirtyCosmicCultComps<T>(EntityUid someUid, T someComp, ComponentStartup ev)
    {
        var cosmicCultComps = AllEntityQuery<CosmicCultComponent>();
        while (cosmicCultComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }

        var cosmicCultLeadComps = AllEntityQuery<CosmicCultLeadComponent>();
        while (cosmicCultLeadComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
    }
}
