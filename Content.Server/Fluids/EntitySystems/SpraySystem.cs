// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 TekuNut <13456422+TekuNut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Golinth <amh2023@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Fluids.Components;
using Content.Server.Gravity;
using Content.Server.Popups;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Interaction;
using Content.Shared.Timing;
using Content.Shared.Vapor;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using System.Numerics;
using Robust.Shared.Map;
using Content.Shared.Inventory; // Assmos - Extinguisher Nozzle
using Content.Shared.Whitelist; // Assmos - Extinguisher Nozzle
using Content.Shared.Hands.EntitySystems; // Assmos - Extinguisher Nozzle

namespace Content.Server.Fluids.EntitySystems;

public sealed class SpraySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly VaporSystem _vapor = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; // Assmos - Extinguisher Nozzle
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!; // Assmos - Extinguisher Nozzle
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!; // Assmos - Extinguisher Nozzle

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SprayComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SprayComponent, UserActivateInWorldEvent>(OnActivateInWorld);
    }

    private void OnActivateInWorld(Entity<SprayComponent> entity, ref UserActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var targetMapPos = _transform.GetMapCoordinates(GetEntityQuery<TransformComponent>().GetComponent(args.Target));

        Spray(entity, args.User, targetMapPos);
    }

    private void OnAfterInteract(Entity<SprayComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var clickPos = _transform.ToMapCoordinates(args.ClickLocation);

        Spray(entity, args.User, clickPos);
    }

    public void Spray(Entity<SprayComponent> entity, EntityUid user, MapCoordinates mapcoord)
    {
        // Assmos - Extinguisher Nozzle
        var sprayOwner = entity.Owner;
        var solutionName = SprayComponent.SolutionName;

        if (entity.Comp.ExternalContainer == true)
        {
            bool foundContainer = false;

            // Check held items (exclude nozzle)
            foreach (var item in _handsSystem.EnumerateHeld(user))
            {
                if (item == entity.Owner)
                {
                    continue;
                }

                if (!_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item) &&
                    _solutionContainer.TryGetSolution(item, SprayComponent.TankSolutionName, out _, out _))
                {
                    sprayOwner = item;
                    solutionName = SprayComponent.TankSolutionName;
                    foundContainer = true;
                    break;
                }
            }

            // Fall back to target slot
            if (!foundContainer && _inventory.TryGetContainerSlotEnumerator(user, out var enumerator, entity.Comp.TargetSlot))
            {
                while (enumerator.NextItem(out var item))
                {
                    if (!_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item) &&
                        _solutionContainer.TryGetSolution(item, SprayComponent.TankSolutionName, out _, out _))
                    {
                        sprayOwner = item;
                        solutionName = SprayComponent.TankSolutionName;
                        foundContainer = true;
                        break;
                    }
                }
            }
        }

        if (!_solutionContainer.TryGetSolution(sprayOwner, solutionName, out var soln, out var solution)) return;
        // End of assmos changes
        //if (!_solutionContainer.TryGetSolution(entity.Owner, SprayComponent.SolutionName, out var soln, out var solution)) return;

        var ev = new SprayAttemptEvent(user);
        RaiseLocalEvent(entity, ref ev);
        if (ev.Cancelled)
            return;

        if (TryComp<UseDelayComponent>(entity, out var useDelay)
            && _useDelay.IsDelayed((entity, useDelay)))
            return;

        if (solution.Volume <= 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("spray-component-is-empty-message"), entity.Owner, user);
            return;
        }

        var xformQuery = GetEntityQuery<TransformComponent>();
        var userXform = xformQuery.GetComponent(user);

        var userMapPos = _transform.GetMapCoordinates(userXform);
        var clickMapPos = mapcoord;

        var diffPos = clickMapPos.Position - userMapPos.Position;
        if (diffPos == Vector2.Zero || diffPos == Vector2Helpers.NaN)
            return;

        // Lavaland Shitcode Start - You should spray yourself NOW.
        // Too lazy to learn this system, so you get a copypaste job!
        if ((clickMapPos.Position - userMapPos.Position).Length() < 0.5f)
        {
            // Split a portion of the solution for the self-spray
            var adjustedSolutionAmount = entity.Comp.TransferAmount;
            var newSolution = _solutionContainer.SplitSolution(soln.Value, adjustedSolutionAmount);

            if (newSolution.Volume > 0)
            {
                // Spawn vapor with a slight offset to create movement
                var offset = new Vector2(0.1f, 0); // Small offset to ensure collision
                var vapor = Spawn(entity.Comp.SprayedPrototype, userMapPos.Offset(offset));
                var vaporXform = xformQuery.GetComponent(vapor);

                if (TryComp(vapor, out AppearanceComponent? appearance))
                {
                    _appearance.SetData(vapor, VaporVisuals.Color, solution.GetColor(_proto).WithAlpha(1f), appearance);
                    _appearance.SetData(vapor, VaporVisuals.State, true, appearance);
                }

                var vaporComponent = Comp<VaporComponent>(vapor);
                var ent = (vapor, vaporComponent);
                _vapor.TryAddSolution(ent, newSolution);

                // Create a slight movement effect
                var rotation = Angle.FromDegrees(45);
                var impulseDirection = -offset.Normalized();
                var time = 0.5f;  // Shorter duration for self-spray
                var target = userMapPos.Offset(impulseDirection * 0.5f);  // Small movement distance

                _vapor.Start(ent, vaporXform, impulseDirection * 0.5f, entity.Comp.SprayVelocity, target, time, user);

                if (TryComp<PhysicsComponent>(user, out var body))
                {
                    if (_gravity.IsWeightless(user, body))
                        _physics.ApplyLinearImpulse(user, -impulseDirection.Normalized() * entity.Comp.PushbackAmount, body: body);
                }

                _audio.PlayPvs(entity.Comp.SpraySound, entity, entity.Comp.SpraySound.Params.WithVariation(0.125f));

                if (useDelay != null)
                    _useDelay.TryResetDelay((entity, useDelay));

                return;
            }
        }
        // Lavaland Shitcode End
        var diffNorm = diffPos.Normalized();
        var diffLength = diffPos.Length();

        if (diffLength > entity.Comp.SprayDistance)
        {
            diffLength = entity.Comp.SprayDistance;
        }

        var diffAngle = diffNorm.ToAngle();

        // Vectors to determine the spawn offset of the vapor clouds.
        var threeQuarters = diffNorm * 0.75f;
        var quarter = diffNorm * 0.25f;

        var amount = Math.Max(Math.Min((solution.Volume / entity.Comp.TransferAmount).Int(), entity.Comp.VaporAmount), 1);
        var spread = entity.Comp.VaporSpread / amount;

        for (var i = 0; i < amount; i++)
        {
            var rotation = new Angle(diffAngle + Angle.FromDegrees(spread * i) -
                                     Angle.FromDegrees(spread * (amount - 1) / 2));

            // Calculate the destination for the vapor cloud. Limit to the maximum spray distance.
            var target = userMapPos
                .Offset((diffNorm + rotation.ToVec()).Normalized() * diffLength + quarter);

            var distance = (target.Position - userMapPos.Position).Length();
            if (distance > entity.Comp.SprayDistance)
                target = userMapPos.Offset(diffNorm * entity.Comp.SprayDistance);

            var adjustedSolutionAmount = entity.Comp.TransferAmount / entity.Comp.VaporAmount;
            var newSolution = _solutionContainer.SplitSolution(soln.Value, adjustedSolutionAmount);

            if (newSolution.Volume <= FixedPoint2.Zero)
                break;

            // Spawn the vapor cloud onto the grid/map the user is present on. Offset the start position based on how far the target destination is.
            var vaporPos = userMapPos.Offset(distance < 1 ? quarter : threeQuarters);
            var vapor = Spawn(entity.Comp.SprayedPrototype, vaporPos);
            var vaporXform = xformQuery.GetComponent(vapor);

            _transform.SetWorldRotation(vaporXform, rotation);

            if (TryComp(vapor, out AppearanceComponent? appearance))
            {
                _appearance.SetData(vapor, VaporVisuals.Color, solution.GetColor(_proto).WithAlpha(1f), appearance);
                _appearance.SetData(vapor, VaporVisuals.State, true, appearance);
            }

            // Add the solution to the vapor and actually send the thing
            var vaporComponent = Comp<VaporComponent>(vapor);
            var ent = (vapor, vaporComponent);
            _vapor.TryAddSolution(ent, newSolution);

            // impulse direction is defined in world-coordinates, not local coordinates
            var impulseDirection = rotation.ToVec();
            var time = diffLength / entity.Comp.SprayVelocity;

            _vapor.Start(ent, vaporXform, impulseDirection * diffLength, entity.Comp.SprayVelocity, target, time, user);

            if (TryComp<PhysicsComponent>(user, out var body))
            {
                if (_gravity.IsWeightless(user, body))
                    _physics.ApplyLinearImpulse(user, -impulseDirection.Normalized() * entity.Comp.PushbackAmount, body: body);
            }
        }

        _audio.PlayPvs(entity.Comp.SpraySound, entity, entity.Comp.SpraySound.Params.WithVariation(0.125f));

        if (useDelay != null)
            _useDelay.TryResetDelay((entity, useDelay));
    }
}