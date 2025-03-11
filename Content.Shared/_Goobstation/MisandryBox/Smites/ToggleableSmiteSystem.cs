namespace Content.Shared._Goobstation.MisandryBox.Smites;

public abstract class ToggleableSmiteSystem<T> : EntitySystem where T : Component
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<T, ComponentInit>(OnInit);
        SubscribeLocalEvent<T, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<T> ent, ref ComponentInit args)
    {
        Set(ent.Owner);
    }

    private void OnShutdown(Entity<T> ent, ref ComponentShutdown args)
    {
        Set(ent.Owner);
    }

    public abstract void Set(EntityUid owner);
}
