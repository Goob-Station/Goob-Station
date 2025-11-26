// SPDX-FileCopyrightText: 2021 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Alerts;
using Content.Shared.Pinpointer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client.Pinpointer;

public sealed class PinpointerSystem : SharedPinpointerSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    // WD EDIT START
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PinpointerComponent, UpdateAlertSpriteEvent>(OnUpdateAlertSprite);
    }

    private void OnUpdateAlertSprite(EntityUid uid, PinpointerComponent component, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != component.Alert)
            return;

        var sprite = args.SpriteViewEnt.Comp;
        var eye = _eyeManager.CurrentEye;
        var angle = component.DistanceToTarget switch
        {
            Distance.Close or Distance.Medium or Distance.Far => component.ArrowAngle + eye.Rotation,
            _ => Angle.Zero
        };

        _sprite.LayerSetRotation((args.SpriteViewEnt, sprite), PinpointerLayers.Screen, angle);
        sprite.LayerSetState(PinpointerLayers.Screen, component.DistanceToTarget.ToString().ToLower());
    }
    // WD EDIT END

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we want to show pinpointers arrow direction relative
        // to players eye rotation (like it was in SS13)

        // because eye can change it rotation anytime
        // we need to update this arrow in a update loop
        var query = EntityQueryEnumerator<PinpointerComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var pinpointer, out var sprite))
        {
            // WD EDIT START
            if (!sprite.LayerExists(PinpointerLayers.Screen))
                continue;
            // WD EDIT END

            if (!pinpointer.HasTarget)
            {
                sprite.LayerSetRotation(PinpointerLayers.Screen, Angle.Zero); // Goob edit
                continue;
            }
            var eye = _eyeManager.CurrentEye;
            var angle = pinpointer.ArrowAngle + eye.Rotation;

            switch (pinpointer.DistanceToTarget)
            {
                case Distance.Close:
                case Distance.Medium:
                case Distance.Far:
                    _sprite.LayerSetRotation((uid, sprite), PinpointerLayers.Screen, angle);
                    break;
                default:
                    _sprite.LayerSetRotation((uid, sprite), PinpointerLayers.Screen, Angle.Zero);
                    break;
            }
        }
    }
}
