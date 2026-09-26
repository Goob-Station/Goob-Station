using Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Originially the code was all here but uhh I was putting a net server check on everything so I just moved it.
/// </summary>

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

public abstract partial class SharedORTTransportMatterSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ORTTransportMatterComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<ORTTransportMatterComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.FadeOutStarted)
        {
            FadeOut(ent);
        }
        else if (ent.Comp.FadeInStarted)
        {
            FadeIn(ent);
        }
    }

    protected virtual void FadeOut(Entity<ORTTransportMatterComponent> ent) { }
    protected virtual void FadeIn(Entity<ORTTransportMatterComponent> ent) { }
}
