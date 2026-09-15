using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Shared.Polymorph;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.NebulaCrawl;

public sealed partial class NebulaCrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedVoidwalkerSystem _voidwalker = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<NebulaCrawlComponent, ExitNebulaCrawlEvent>(OnExitNebulaCrawl);
    }

    private void OnExitNebulaCrawl(Entity<NebulaCrawlComponent> entity, ref ExitNebulaCrawlEvent args)
    {
        if (args.Handled)
            return;

        if (!_voidwalker.CheckInSpace(entity))
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
