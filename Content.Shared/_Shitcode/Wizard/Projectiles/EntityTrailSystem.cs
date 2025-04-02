namespace Content.Shared._Goobstation.Wizard.Projectiles;

public sealed class EntityTrailSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityTrailComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<EntityTrailComponent> ent, ref ComponentInit args)
    {
        var (uid, comp) = ent;
        if (!TryComp(uid, out TrailComponent? trail))
            return;

        trail.RenderedEntity = uid;
        Dirty(uid, trail);
    }
}
