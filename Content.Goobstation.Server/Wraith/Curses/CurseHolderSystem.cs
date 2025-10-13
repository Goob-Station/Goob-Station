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
        // TODO: popup here
        RemCompDeferred<CurseHolderComponent>(ent.Owner);
    }
}
