

using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
namespace Content.Goobstation.Shared.CustomLawboard;

public abstract class SharedCustomLawboardSystem : SharedSiliconLawSystem
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
        var lawset = new SiliconLawset
        {
            Laws = args.Laws
        };
        provider.Lawset = lawset;
        _adminLogger.Add(LogType.Unknown, $"{ToPrettyString(args.Actor)} changed laws on {ToPrettyString(uid)}"); // TODO IN A BIT DONT FORGET PLEASE
        Dirty(uid, customLawboard);
        DirtyUI(uid, customLawboard);
    }

    protected virtual void DirtyUI(EntityUid uid, CustomLawboardComponent? customLawboard, UserInterfaceComponent? ui = null) { }
}
