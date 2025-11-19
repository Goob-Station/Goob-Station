using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Wraith.Curses;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Wraith.Curses;

public sealed class CurseHolderSystem : SharedCurseHolderSystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseHolderComponent, BibleSmiteUsed>(OnBibleSmite);
    }

    private void OnBibleSmite(Entity<CurseHolderComponent> ent, ref BibleSmiteUsed args)
    {
        _popupSystem.PopupEntity(Loc.GetString("curse-not-anymore"), ent.Owner, ent.Owner, PopupType.Medium);
        RemCompDeferred<CurseHolderComponent>(ent.Owner);
    }
}
