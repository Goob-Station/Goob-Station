

using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Chat;


namespace Content.Goobstation.Shared.CustomLawboard;

public abstract class SharedCustomLawboardSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public static readonly int MaxLaws = 15;
    public static readonly int MaxLawLength = 512; // These 2 are random arbitrary numbers (These don't seem like they're worth making cvars for)
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomLawboardComponent, CustomLawboardChangeLawsMessage>(OnChangeLaws);
        SubscribeLocalEvent<CustomLawboardComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(Entity<CustomLawboardComponent> ent, ref ComponentInit args)
    {
        var provider = EnsureComp<SiliconLawProviderComponent>(ent);

        // This part was shamelessly stolen from SiliconLawSystem.GetLaws

        var proto = _prototype.Index(provider.Laws);
        var laws = new SiliconLawset()
        {
            Laws = new List<SiliconLaw>(proto.Laws.Count)
        };
        foreach (var law in proto.Laws)
        {
            laws.Laws.Add(_prototype.Index<SiliconLawPrototype>(law).ShallowClone());
        }
        laws.ObeysTo = proto.ObeysTo;

        ent.Comp.Laws = laws.Laws;
        provider.Lawset = laws;
    }

    public List<SiliconLaw> SanitizeLaws(List<SiliconLaw> listToSanitize)
    {
        var sanitizedLaws = new List<SiliconLaw>();
        foreach (SiliconLaw law in listToSanitize)
        {
            // SanitizeAnnouncement gets the job done so who really cares honestly
            var sanitizedLaw = SharedChatSystem.SanitizeAnnouncement(law.LawString, MaxLawLength, 0);
            sanitizedLaws.Add(new SiliconLaw()
            {
                LawString = sanitizedLaw,
                Order = law.Order,
                LawIdentifierOverride = law.LawIdentifierOverride
            });
        }
        return sanitizedLaws;
    }

    private void OnChangeLaws(EntityUid uid, CustomLawboardComponent customLawboard, CustomLawboardChangeLawsMessage args)
    {
        var provider = EnsureComp<SiliconLawProviderComponent>(uid);
        var lawset = new SiliconLawset();
        var sanitizedLaws = SanitizeLaws(args.Laws);
        lawset.Laws = sanitizedLaws; // Sanitizing is done so you can't make newlines in a law.

        customLawboard.Laws = sanitizedLaws;
        provider.Lawset = lawset;
        _adminLogger.Add(LogType.Action, $"{ToPrettyString(args.Actor)} changed laws on {ToPrettyString(uid)}");
        Dirty(uid, customLawboard);
    }

    protected virtual void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard, UserInterfaceComponent? ui = null) { }
}
