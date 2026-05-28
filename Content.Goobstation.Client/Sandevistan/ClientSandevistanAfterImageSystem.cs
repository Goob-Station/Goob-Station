using System.Numerics;
using Content.Goobstation.Shared.Sandevistan;
using Content.Shared.Tag;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using DrawDepthEnum = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Goobstation.Client.Sandevistan;

public sealed class ClientSandevistanAfterimageSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private static readonly ProtoId<TagPrototype> HideContextMenuTag = "HideContextMenu";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanAfterimageComponent, ComponentStartup>(OnAfterimageStartup);
    }

    private void OnAfterimageStartup(Entity<SandevistanAfterimageComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent.Comp.SourceEntity, out var userSprite))
            return;

        _tagSystem.AddTag(ent, HideContextMenuTag);

        var afterimageSprite = EnsureComp<SpriteComponent>(ent);
        _sprite.CopySprite((ent.Comp.SourceEntity, userSprite), (ent.Owner, afterimageSprite));
        _sprite.SetDrawDepth((ent.Owner, afterimageSprite), (int) DrawDepthEnum.FloorEffects);
        _sprite.SetColor((ent.Owner, afterimageSprite), Color.FromHsv(new Vector4(ent.Comp.Hue, 1, 1, 0.7f)));
        afterimageSprite.PostShader = null;
        afterimageSprite.RenderOrder = 0;
        afterimageSprite.EnableDirectionOverride = true;
        afterimageSprite.DirectionOverride = ent.Comp.DirectionOverride;
    }
}
