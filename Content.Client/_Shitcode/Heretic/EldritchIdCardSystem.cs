using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class EldritchIdCardSystem : SharedEldritchIdCardSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EldritchIdCardComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<EldritchIdCardComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.CurrentProto == null)
            return;

        var dummy = Spawn(ent.Comp.CurrentProto);
        _sprite.CopySprite(dummy, ent.Owner);
        Del(dummy);
    }
}
