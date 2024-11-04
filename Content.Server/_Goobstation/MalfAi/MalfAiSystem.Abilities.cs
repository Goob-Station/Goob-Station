using Content.Shared.MalfAi;
using Content.Shared.Mobs;
using Content.Shared.DoAfter;
using Content.Shared.Store.Components;
using Content.Shared.Popups;

namespace Content.Server.MalfAi;

public sealed partial class MalfAiSystem : EntitySystem
{
    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<MalfAiComponent, OpenModuleMenuEvent>(OnOpenModuleMenu);
    }


    private void OnOpenModuleMenu(EntityUid uid, MalfAiComponent comp, ref OpenModuleMenuEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }
    /* will do it soon
    private void OnCyborgHijack(EntityUid uid, MalfAiComponent comp, ref CyborgHijackEvent args)
    {
        if (!TryCyborgHijack(uid, comp, args, true))
            return;

        var target = args.Target;
    }
    */
}
