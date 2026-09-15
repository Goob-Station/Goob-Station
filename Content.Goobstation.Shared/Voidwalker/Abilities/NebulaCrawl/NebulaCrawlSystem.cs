using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.Spaced;
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.NebulaCrawl;

public sealed partial class NebulaCrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<NebulaCrawlComponent, ComponentStartup>(OnNebulaCrawlStartup);
        SubscribeLocalEvent<NebulaCrawlComponent, ComponentShutdown>(OnNebulaCrawlShutdown);

        SubscribeLocalEvent<NebulaCrawlComponent, ExitNebulaCrawlEvent>(OnExitNebulaCrawl);
    }

    private void OnNebulaCrawlStartup(Entity<NebulaCrawlComponent> entity, ref ComponentStartup args)
    {
        _actions.AddAction(entity, ref entity.Comp.NebulaCrawlActionEntity, entity.Comp.NebulaCrawlAction);
    }

    private void OnNebulaCrawlShutdown(Entity<NebulaCrawlComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.NebulaCrawlActionEntity);
    }

    private void OnExitNebulaCrawl(Entity<NebulaCrawlComponent> entity, ref ExitNebulaCrawlEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<SpacedStatusComponent>(entity, out var spaced)
            && !spaced.IsInSpace)
        {
            var popup = Loc.GetString("voidwalker-action-fail-require-in-space");
            _popup.PopupEntity(popup, entity, entity);

            return;
        }

        var ev = new RevertPolymorphActionEvent(); // Shared woes.
        RaiseLocalEvent(entity, ev);

        args.Handled = true;
    }


}
