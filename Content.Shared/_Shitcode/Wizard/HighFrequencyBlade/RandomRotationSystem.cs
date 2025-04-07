// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared._Goobstation.Wizard.HighFrequencyBlade;

public sealed class RandomRotationSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomRotationComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<RandomRotationComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsServer || IsClientSide(ent))
            _transform.SetLocalRotation(ent, _random.NextAngle());
    }
}