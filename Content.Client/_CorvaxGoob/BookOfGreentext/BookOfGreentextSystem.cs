using Content.Shared._CorvaxGoob.BookOfGreentext;
using Robust.Client.GameObjects;

namespace Content.Client._CorvaxGoob.BookOfGreentext;

public sealed class BookOfGreentextSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseOfBookOfGreentextComponent, ComponentShutdown>(OnComponentShutdown);
    }

    public void OnComponentShutdown(Entity<CurseOfBookOfGreentextComponent> entity, ref ComponentShutdown ev)
    {
        if (TryComp<SpriteComponent>(entity, out var sprite))
            _sprite.SetColor((entity.Owner, sprite), Color.White);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = AllEntityQuery<CurseOfBookOfGreentextComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var curse, out var sprite))
            _sprite.SetColor(uid, curse.Completed ? Color.LightGreen : Color.IndianRed);
    }
}
