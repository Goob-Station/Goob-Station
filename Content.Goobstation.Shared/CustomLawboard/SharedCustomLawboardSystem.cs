

using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CustomLawboard;

public abstract class SharedCustomLawboardSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
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
