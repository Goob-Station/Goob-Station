// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StatusEffectNew;
using Content.Trauma.Shared.StatusEffects;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Trauma.Client.StatusEffects;

public sealed class AddStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddShaderStatusEffectComponent, StatusEffectAppliedEvent>(OnStartUp);
        SubscribeLocalEvent<AddShaderStatusEffectComponent, StatusEffectRemovedEvent>(OnShutdown);
    }

    private void OnShutdown(Entity<AddShaderStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!Terminating(args.Target))
            SetShader(args.Target, false, ent.Comp.Shader);
    }

    private void OnStartUp(Entity<AddShaderStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<SpriteComponent>(args.Target, out var sprite))
            return;

        SetShader(args.Target, true, ent.Comp.Shader);
    }

    private void SetShader(EntityUid uid, bool enabled, ProtoId<ShaderPrototype> shaderproto)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var shader = _protoMan.Index(shaderproto).InstanceUnique();

        sprite.PostShader = enabled ? shader : null;
        sprite.GetScreenTexture = enabled;
        sprite.RaiseShaderEvent = enabled;
    }
}
