// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.Ghost.Roles;

namespace Content.Server.Ghost.Roles.UI
{
    public sealed class MakeGhostRoleEui : BaseEui
    {
        private IEntityManager _entManager;

        public MakeGhostRoleEui(IEntityManager entManager, NetEntity entity)
        {
            _entManager = entManager;
            Entity = entity;
        }

        public NetEntity Entity { get; }

        public override EuiStateBase GetNewState()
        {
            return new MakeGhostRoleEuiState(Entity);
        }

        public override void Closed()
        {
            base.Closed();

            _entManager.System<GhostRoleSystem>().CloseMakeGhostRoleEui(Player);
        }
    }
}