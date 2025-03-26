using System.Linq;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class BindSoulSystem : SharedBindSoulSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhylacteryComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<PhylacteryComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        var color = Color.FromHex("#003300");

        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            sprite.LayerSetColor(i, color);
        }
    }
}
