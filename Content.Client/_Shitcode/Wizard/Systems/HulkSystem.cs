using System.Linq;
using Content.Shared._Goobstation.Wizard.Mutate;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class HulkSystem : SharedHulkSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HulkComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<HulkComponent> ent, ref ComponentShutdown args)
    {
        var (uid, comp) = ent;

        if (TerminatingOrDeleted(uid))
            return;

        if (HasComp<HumanoidAppearanceComponent>(uid))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var layerCount = sprite.AllLayers.Count();

        if (comp.NonHumanoidOldLayerData.Count != layerCount)
            return;

        for (var i = 0; i < layerCount; i++)
        {
            sprite.LayerSetColor(i, comp.NonHumanoidOldLayerData[i]);
        }
    }

    protected override void UpdateColorStartup(Entity<HulkComponent> hulk)
    {
        base.UpdateColorStartup(hulk);

        var (uid, comp) = hulk;

        if (HasComp<HumanoidAppearanceComponent>(uid))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            if (!sprite.TryGetLayer(i, out var layer))
                return;
            comp.NonHumanoidOldLayerData.Add(layer.Color);
            sprite.LayerSetColor(i, comp.SkinColor);
        }
    }
}
