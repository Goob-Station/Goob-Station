// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Beam;
using Content.Shared.Beam.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Beam;

public sealed class BeamSystem : SharedBeamSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<BeamVisualizerEvent>(BeamVisualizerMessage);
    }

    //TODO: Sometime in the future this needs to be replaced with tiled sprites
    private void BeamVisualizerMessage(BeamVisualizerEvent args)
    {
        var beam = GetEntity(args.Beam);

        if (TryComp<SpriteComponent>(beam, out var sprites))
        {
            _sprite.SetRotation((beam, sprites), args.UserAngle);

            if (args.BodyState != null)
            {
                _sprite.LayerSetRsiState((beam, sprites), 0, args.BodyState);
                sprites.LayerSetShader(0, args.Shader);
            }
        }
    }
}