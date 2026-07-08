// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class BindSoulSystem : SharedBindSoulSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

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
        var drawDepth = (int) Shared.DrawDepth.DrawDepth.Items;

        if (sprite.DrawDepth < drawDepth)
            _sprite.SetDrawDepth((ent.Owner, sprite), drawDepth);

        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            _sprite.LayerSetColor((ent.Owner, sprite), i, color);
        }
    }
}
