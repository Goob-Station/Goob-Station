using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Wraith.Curses;

namespace Content.Goobstation.Server.Wraith.Curses;

public sealed class CurseHolderSystem : SharedCurseHolderSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseHolderComponent, BibleSmiteUsed>(OnBibleSmite);
    }

    private void OnBibleSmite(Entity<CurseHolderComponent> ent, ref BibleSmiteUsed args)
    {
        // reset everything
        ent.Comp.ActiveCurses.Clear();
        ent.Comp.CurseUpdate.Clear();
        ent.Comp.CurseStatusIcons.Clear();
        ent.Comp.Curser = null;

        // popup here
        Dirty(ent);
    }
}
