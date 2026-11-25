// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Graphics;
using Content.Shared.Nyanotrasen.Holograms;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.Nyanotrasen.Holograms;

public sealed class HologramVisualizerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private const string PostShaderId = "hologram";

    private readonly ProtoId<ShaderPrototype> _shaderId = "Holographic"; // Goobstation - Start
    private ShaderPrototype? _shaderProto;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HologramVisualsComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<HologramVisualsComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentStartup(EntityUid uid, HologramVisualsComponent component, ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            _sprite.SetPostShader((uid, sprite), new SpriteComponent.PostShaderArgs(PostShaderId, (_shaderProto ??= _protoMan.Index(_shaderId)).InstanceUnique())
            {
                Before = ContentPostShaderIds.BeforeOutlines,
            });
    }

    private void OnComponentShutdown(EntityUid uid, HologramVisualsComponent component, ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            _sprite.RemovePostShader((uid, sprite), PostShaderId);
    } // Goobstation - End
}
