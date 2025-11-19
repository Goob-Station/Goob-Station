// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Maps;

[ImplicitDataDefinitionForInheritors]
public abstract partial class GameMapCondition
{
    [DataField("inverted")]
    public bool Inverted { get; private set; }
    public abstract bool Check(GameMapPrototype map);
}
