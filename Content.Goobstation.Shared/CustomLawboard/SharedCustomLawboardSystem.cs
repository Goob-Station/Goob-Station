using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;


namespace Content.Goobstation.Shared.CustomLawboard;

public abstract class SharedCustomLawboardSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public static readonly int MaxLaws = 15;
    public static readonly int MaxLawLength = 512; // These 2 are random arbitrary numbers (These don't seem like they're worth making cvars for)
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomLawboardComponent, CustomLawboardChangeLawsMessage>(OnChangeLaws);
    }

    public List<SiliconLaw> SanitizeLaws(List<SiliconLaw> listToSanitize)
    {
        var sanitizedLaws = new List<SiliconLaw>();
        foreach (SiliconLaw law in listToSanitize)
        {

            var sanitizedLaw = law.LawString.Replace("\n", " "); // Remove newlines cause they mess chat up when the law is stated
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

        if (args.Popup)
        {
            _popup.PopupClient(Loc.GetString("custom-lawboard-updated"), args.Actor, args.Actor); // This is entirely to make the UI feel responsive
        }
    }

    protected virtual void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard, UserInterfaceComponent? ui = null) { }
}
