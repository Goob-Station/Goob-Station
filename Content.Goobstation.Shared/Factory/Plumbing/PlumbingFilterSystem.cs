using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory.Plumbing;

public sealed class PlumbingFilterSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<PlumbingFilterComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<PlumbingFilterComponent>();

        SubscribeLocalEvent<PlumbingFilterComponent, ExaminedEvent>(OnExamined);
        Subs.BuiEvents<PlumbingFilterComponent>(PlumbingFilterUiKey.Key, subs =>
        {
            subs.Event<PlumbingFilterChangeMessage>(OnChange);
        });
    }

    private void OnExamined(Entity<PlumbingFilterComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (ent.Comp.Filter is not {} filter)
        {
            args.PushMarkup(Loc.GetString("plumbing-filter-unset"));
            return;
        }

        var name = _proto.Index(filter).LocalizedName;
        args.PushMarkup(Loc.GetString("plumbing-filter-set", ("reagent", name)));
    }

    private void OnChange(Entity<PlumbingFilterComponent> ent, ref PlumbingFilterChangeMessage args)
    {
        if (args.Filter == ent.Comp.Filter
            || (args.Filter is {} filter
            && !_proto.HasIndex(filter)))
            return;

        var msg = args.Filter is {} filter2 // chud language
            ? Loc.GetString("plumbing-filter-changed", ("reagent", _proto.Index(filter2).LocalizedName))
            : Loc.GetString("plumbing-filter-removed");
        _popup.PopupClient(msg, ent, args.Actor);

        ent.Comp.Filter = args.Filter;
        Dirty(ent);
    }

    public ProtoId<ReagentPrototype>? GetFilteredReagent(EntityUid uid)
        => _query.CompOrNull(uid)?.Filter;
}
