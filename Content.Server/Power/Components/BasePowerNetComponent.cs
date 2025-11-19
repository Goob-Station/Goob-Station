// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Server.Power.NodeGroups;

namespace Content.Server.Power.Components
{
    public interface IBasePowerNetComponent : IBaseNetConnectorComponent<IPowerNet>
    {

    }

    public abstract partial class BasePowerNetComponent : BaseNetConnectorComponent<IPowerNet>
    {
    }
}
