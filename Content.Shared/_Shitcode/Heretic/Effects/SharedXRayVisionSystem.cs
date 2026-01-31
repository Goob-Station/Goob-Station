namespace Content.Shared.Heretic.Effects;

public abstract class SharedXRayVisionSystem : EntitySystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XRayVisionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<XRayVisionComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<XRayVisionComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent) || !TryComp(ent, out EyeComponent? eye))
            return;

        _eye.SetDrawFov(ent, ent.Comp.EyeHadFov, eye);
        DrawLight(true);
    }

    private void OnStartup(Entity<XRayVisionComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp(ent, out EyeComponent? eye))
            return;

        ent.Comp.EyeHadFov = eye.DrawFov;
        _eye.SetDrawFov(ent, false, eye);
        DrawLight(false);
    }

    protected virtual void DrawLight(bool value) { }
}
