using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Emoting;
public abstract class SharedFartSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FartComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(Entity<FartComponent> ent, ref ComponentGetState args)
    {
        args.State = new FartComponentState(ent.Comp.Emote, ent.Comp.FartTimeout, ent.Comp.FartInhale, ent.Comp.SuperFarted);
    }
}
