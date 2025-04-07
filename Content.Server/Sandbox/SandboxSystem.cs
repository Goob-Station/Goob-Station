// SPDX-FileCopyrightText: 2020 F77F <66768086+F77F@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 scuffedjays <yetanotherscuffed@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <vincefvanwijk@gmail.com>
// SPDX-FileCopyrightText: 2021 Wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Sandbox;
using Robust.Server.Console;
using Robust.Server.Placement;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Sandbox
{
    public sealed class SandboxSystem : SharedSandboxSystem
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IPlacementManager _placementManager = default!;
        [Dependency] private readonly IConGroupController _conGroupController = default!;
        [Dependency] private readonly IServerConsoleHost _host = default!;
        [Dependency] private readonly SharedAccessSystem _access = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;
        [Dependency] private readonly ItemSlotsSystem _slots = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

        private bool _isSandboxEnabled;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsSandboxEnabled
        {
            get => _isSandboxEnabled;
            set
            {
                _isSandboxEnabled = value;
                UpdateSandboxStatusForAll();
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            SubscribeNetworkEvent<MsgSandboxRespawn>(SandboxRespawnReceived);
            SubscribeNetworkEvent<MsgSandboxGiveAccess>(SandboxGiveAccessReceived);
            SubscribeNetworkEvent<MsgSandboxGiveAghost>(SandboxGiveAghostReceived);
            SubscribeNetworkEvent<MsgSandboxSuicide>(SandboxSuicideReceived);

            SubscribeLocalEvent<GameRunLevelChangedEvent>(GameTickerOnOnRunLevelChanged);

            _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

            _placementManager.AllowPlacementFunc = placement =>
            {
                if (IsSandboxEnabled)
                {
                    return true;
                }

                var channel = placement.MsgChannel;
                var player = _playerManager.GetSessionByChannel(channel);

                if (_conGroupController.CanAdminPlace(player))
                {
                    return true;
                }

                return false;
            };
        }

        public override void Shutdown()
        {
            base.Shutdown();
            _placementManager.AllowPlacementFunc = null;
            _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
        }

        private void GameTickerOnOnRunLevelChanged(GameRunLevelChangedEvent obj)
        {
            // Automatically clear sandbox state when round resets.
            if (obj.New == GameRunLevel.PreRoundLobby)
            {
                IsSandboxEnabled = false;
            }
        }

        private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
        {
            if (e.NewStatus != SessionStatus.Connected || e.OldStatus != SessionStatus.Connecting)
                return;

            RaiseNetworkEvent(new MsgSandboxStatus { SandboxAllowed = IsSandboxEnabled }, e.Session.Channel);
        }

        private void SandboxRespawnReceived(MsgSandboxRespawn message, EntitySessionEventArgs args)
        {
            if (!IsSandboxEnabled)
                return;

            var player = _playerManager.GetSessionByChannel(args.SenderSession.Channel);
            if (player.AttachedEntity == null) return;

            _ticker.Respawn(player);
        }

        private void SandboxGiveAccessReceived(MsgSandboxGiveAccess message, EntitySessionEventArgs args)
        {
            if (!IsSandboxEnabled)
                return;

            var player = _playerManager.GetSessionByChannel(args.SenderSession.Channel);
            if (player.AttachedEntity is not { } attached)
            {
                return;
            }

            var allAccess = PrototypeManager
                .EnumeratePrototypes<AccessLevelPrototype>()
                .Select(p => new ProtoId<AccessLevelPrototype>(p.ID)).ToList();

            if (_inventory.TryGetSlotEntity(attached, "id", out var slotEntity))
            {
                if (HasComp<AccessComponent>(slotEntity))
                {
                    UpgradeId(slotEntity.Value);
                }
                else if (TryComp<PdaComponent>(slotEntity, out var pda))
                {
                    if (pda.ContainedId is null)
                    {
                        var newID = CreateFreshId();
                        if (TryComp<ItemSlotsComponent>(slotEntity, out var itemSlots))
                        {
                            _slots.TryInsert(slotEntity.Value, pda.IdSlot, newID, null);
                        }
                    }
                    else
                    {
                        UpgradeId(pda.ContainedId!.Value);
                    }
                }
            }
            else if (TryComp<HandsComponent>(attached, out var hands))
            {
                var card = CreateFreshId();
                if (!_inventory.TryEquip(attached, card, "id", true, true))
                {
                    _handsSystem.PickupOrDrop(attached, card, handsComp: hands);
                }
            }

            void UpgradeId(EntityUid id)
            {
                _access.TrySetTags(id, allAccess);
            }

            EntityUid CreateFreshId()
            {
                var card = Spawn("CaptainIDCard", Transform(attached).Coordinates);
                UpgradeId(card);

                Comp<IdCardComponent>(card).FullName = MetaData(attached).EntityName;
                return card;
            }
        }

        private void SandboxGiveAghostReceived(MsgSandboxGiveAghost message, EntitySessionEventArgs args)
        {
            if (!IsSandboxEnabled)
                return;

            var player = _playerManager.GetSessionByChannel(args.SenderSession.Channel);

            _host.ExecuteCommand(player, _conGroupController.CanCommand(player, "aghost") ? "aghost" : "ghost");
        }

        private void SandboxSuicideReceived(MsgSandboxSuicide message, EntitySessionEventArgs args)
        {
            if (!IsSandboxEnabled)
                return;

            var player = _playerManager.GetSessionByChannel(args.SenderSession.Channel);
            _host.ExecuteCommand(player, "suicide");
        }

        private void UpdateSandboxStatusForAll()
        {
            RaiseNetworkEvent(new MsgSandboxStatus { SandboxAllowed = IsSandboxEnabled });
        }
    }
}