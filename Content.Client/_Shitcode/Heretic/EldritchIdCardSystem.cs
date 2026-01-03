using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.UserInterface;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class EldritchIdCardSystem : SharedEldritchIdCardSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EldritchIdCardComponent, ComponentStartup>(OnStartup, before: new []{ typeof(ActivatableUISystem) });
    }

    private void OnStartup(Entity<EldritchIdCardComponent> ent, ref ComponentStartup args)
    {
        // Prevent clientside errors
        // TODO: this doesn't work
        var ui = EnsureComp<ActivatableUIComponent>(ent.Owner);
        ui.Key = EldritchIdUiKey.Key;
        Dirty(ent.Owner, ui);

        UpdateSprite(ent);
    }

    protected override void UpdateSprite(Entity<EldritchIdCardComponent> ent)
    {
        if (ent.Comp.CurrentProto == null)
            return;

        var dummy = Spawn(ent.Comp.CurrentProto);
        _sprite.CopySprite(dummy, ent.Owner);
        Del(dummy);
    }
}
