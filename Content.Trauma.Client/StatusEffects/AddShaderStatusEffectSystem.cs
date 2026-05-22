// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StatusEffectNew;
using Content.Trauma.Shared.StatusEffects;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Trauma.Client.StatusEffects;

public sealed partial class AddShaderStatusEffectSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    private EntityQuery<SpriteComponent> _spriteQuery;

    public override void Initialize()
    {
        base.Initialize();

        _spriteQuery = GetEntityQuery<SpriteComponent>();

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
        if (!_spriteQuery.HasComp(args.Target))
            return;

        SetShader(args.Target, true, ent.Comp.Shader);
    }

    private void SetShader(EntityUid uid, bool enabled, ProtoId<ShaderPrototype> shaderproto)
    {
        if (!_spriteQuery.TryComp(uid, out var sprite))
            return;

        if (enabled)
        {
            var shader = _proto.Index(shaderproto).Instance();
            sprite.PostShader = shader;
        }

        sprite.GetScreenTexture = enabled;
        sprite.RaiseShaderEvent = enabled;
    }
}
