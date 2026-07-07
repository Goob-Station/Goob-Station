using System.Linq;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Popups;
using Content.Shared.Salvage.Fulton;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Salvage.Fulton;
public sealed class EmaggedFultonSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FultonComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid ent, FultonComponent comp, ref GotEmaggedEvent args)
    {
        if (TryComp<EmaggedComponent>(ent, out var emaggedComp))
            return;
        args.Handled = true;
        ChangeWhitelistToEvac(comp, "MindContainer"); // All mobs can be extracted by fulton after emagging
        _popup.PopupEntity(Loc.GetString("fulton-emagged"), ent);
        _audio.PlayPredicted(comp.FultonSoundEmag, ent, ent);
    }


    // Adding new Comp to whitelist for evac
    public void ChangeWhitelistToEvac(FultonComponent comp,string nameComp)
    {
        if (comp.Whitelist == null || comp.Whitelist.Components == null)
            return;
        comp.Whitelist.Components=comp.Whitelist.Components
        .Union(new[] { nameComp })
        .ToArray();
    }
}
