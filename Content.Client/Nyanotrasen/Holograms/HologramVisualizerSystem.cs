// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Nyanotrasen.Holograms;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.Nyanotrasen.Holograms;

public sealed class HologramVisualizerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private readonly ProtoId<ShaderPrototype> _shaderId = "Holographic"; // Goobstation - Start
    private ShaderPrototype? _shaderProto;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HologramVisualsComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<HologramVisualsComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentInit(EntityUid uid, HologramVisualsComponent component, ComponentInit args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.PostShader = (_shaderProto ??= _protoMan.Index(_shaderId)).InstanceUnique();
    }

    private void OnComponentShutdown(EntityUid uid, HologramVisualsComponent component, ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.PostShader = null;
    } // Goobstation - End
}
