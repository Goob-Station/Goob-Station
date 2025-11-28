

using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Utility;
using System.Linq;
namespace Content.Goobstation.Shared.CustomLawboard;

public abstract class SharedCustomLawboardSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomLawboardComponent, CustomLawboardChangeLawsMessage>(OnChangeLaws);
    }

    private void OnChangeLaws(EntityUid uid, CustomLawboardComponent customLawboard, CustomLawboardChangeLawsMessage args)
    {
        var provider = EnsureComp<SiliconLawProviderComponent>(uid);
        var lawset = new SiliconLawset();
        lawset.Laws = args.Laws;

        customLawboard.Laws = args.Laws;
        provider.Lawset = lawset;
        _adminLogger.Add(LogType.Action, $"{ToPrettyString(args.Actor)} changed laws on {ToPrettyString(uid)}");
        Dirty(uid, customLawboard);
    }

    protected virtual void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard, UserInterfaceComponent? ui = null) { }
}
