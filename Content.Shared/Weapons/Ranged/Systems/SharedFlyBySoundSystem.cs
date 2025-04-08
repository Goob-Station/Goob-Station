// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <43253663+DogZeroX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <le0nel_1van@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract class SharedFlyBySoundSystem : EntitySystem
{
    [Dependency] private readonly FixtureSystem _fixtures = default!;

    public const string FlyByFixture = "fly-by";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FlyBySoundComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlyBySoundComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, FlyBySoundComponent component, ComponentStartup args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var body))
            return;

        var shape = new PhysShapeCircle(component.Range);

        _fixtures.TryCreateFixture(uid, shape, FlyByFixture, collisionLayer: (int) CollisionGroup.MobMask, hard: false, body: body);
    }

    private void OnShutdown(EntityUid uid, FlyBySoundComponent component, ComponentShutdown args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var body) ||
            MetaData(uid).EntityLifeStage >= EntityLifeStage.Terminating)
        {
            return;
        }

        _fixtures.DestroyFixture(uid, FlyByFixture, body: body);
    }
}