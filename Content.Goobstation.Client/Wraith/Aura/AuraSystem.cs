using Content.Goobstation.Client.Shaders;
using Content.Goobstation.Common.Shaders;
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
        SubscribeLocalEvent<AuraComponent, BeforePostMultiShaderRenderEvent>(OnMultiRender);
    }

    private void OnStartup(Entity<AuraComponent> ent, ref ComponentStartup args) =>
        SetShader(ent.AsNullable(), true);

    private void OnShutdown(Entity<AuraComponent> ent, ref ComponentShutdown args)
    {
        if (!Terminating(ent.Owner))
            SetShader(ent.AsNullable(), false);
    }

    private void SetShader(Entity<AuraComponent?, SpriteComponent?> ent, bool enabled)
    {
        if (!Resolve(ent.Owner, ref ent.Comp1, ref ent.Comp2, false))
            return;

        if (ent.Comp1.MultiShaderOrder is { } order)
        {
            var ev = new SetMultiShaderEvent(Shader, enabled, order, null, false, true);
            RaiseLocalEvent(ent, ref ev);
            return;
        }

        ent.Comp2.PostShader = enabled ? _shader : null;
        ent.Comp2.GetScreenTexture = enabled;
        ent.Comp2.RaiseShaderEvent = enabled;
    }


    private void OnMultiRender(Entity<AuraComponent> ent, ref BeforePostMultiShaderRenderEvent args)
    {
        if (args.Shader != Shader)
            return;

        args.Instance.SetParameter("distortion", ent.Comp.Distortion);
        args.Instance.SetParameter("auraColor", new Vector3(ent.Comp.AuraColor.R, ent.Comp.AuraColor.G, ent.Comp.AuraColor.B));
        args.Instance.SetParameter("mango", ent.Comp.AuraFarm);
    }

    private void OnShaderRender(Entity<AuraComponent> ent, ref BeforePostShaderRenderEvent args)
    {
        if (args.Sprite.PostShader != _shader)
            return;

        _shader.SetParameter("distortion", ent.Comp.Distortion);
        _shader.SetParameter("auraColor", new Vector3(ent.Comp.AuraColor.R, ent.Comp.AuraColor.G, ent.Comp.AuraColor.B));
        _shader.SetParameter("mango", ent.Comp.AuraFarm);
    }
}
