using Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Originially the code was all here but uhh I was putting a net server check on everything so I just moved it.
/// </summary>

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

public abstract partial class SharedFadingAnchoredTeleportSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FadingAnchoredTeleportComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<FadingAnchoredTeleportComponent> ent, ref AfterAutoHandleStateEvent args)
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

    protected virtual void FadeOut(Entity<FadingAnchoredTeleportComponent> ent) { }
    protected virtual void FadeIn(Entity<FadingAnchoredTeleportComponent> ent) { }
}
