using Content.Goobstation.Shared.Wraith.Aura;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Wraith.Aura;

/// <summary>
/// This be handling your aura 🥀
/// </summary>
public sealed class AuraSystem : EntitySystem
{
    private static readonly ProtoId<ShaderPrototype> Shader = "Aura";

    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private ShaderInstance _shader = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index(Shader).InstanceUnique();

        SubscribeLocalEvent<AuraComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AuraComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AuraComponent, BeforePostShaderRenderEvent>(OnShaderRender);
    }

    private void OnStartup(Entity<AuraComponent> ent, ref ComponentStartup args) =>
        SetShader((ent.Owner, ent.Comp), true);

    private void OnShutdown(Entity<AuraComponent> ent, ref ComponentShutdown args)
    {
        if (!Terminating(ent.Owner))
            SetShader((ent.Owner, ent.Comp), false);
    }

    private void SetShader(Entity<AuraComponent?> ent, bool enabled, SpriteComponent? sprite = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, ref sprite, false))
            return;

        sprite.PostShader = enabled ? _shader : null;
        sprite.GetScreenTexture = enabled;
        sprite.RaiseShaderEvent = enabled;
    }

    private void OnShaderRender(Entity<AuraComponent> ent, ref BeforePostShaderRenderEvent args)
    {
        _shader.SetParameter("distortion", ent.Comp.Distortion);
        _shader.SetParameter("auraColor", new Vector3(ent.Comp.AuraColor.A, ent.Comp.AuraColor.R, ent.Comp.AuraColor.G));
        _shader.SetParameter("mango", ent.Comp.AuraFarm);
    }
}
