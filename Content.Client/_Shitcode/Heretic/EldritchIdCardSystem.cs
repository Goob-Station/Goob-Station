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

        SubscribeLocalEvent<EldritchIdCardComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EldritchIdCardComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<EldritchIdCardComponent> ent, ref ComponentInit args)
    {
        UpdateActivatableUi(ent);
    }

    private void OnStartup(Entity<EldritchIdCardComponent> ent, ref ComponentStartup args)
    {
        UpdateActivatableUi(ent);
        UpdateSprite(ent);
    }

    // Required so that ActivatableUiSystem doesn't log errors clientside
    private void UpdateActivatableUi(EntityUid uid)
    {
        var ui = EnsureComp<ActivatableUIComponent>(uid);
        ui.Key = EldritchIdUiKey.Key;
        Dirty(uid, ui);
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
