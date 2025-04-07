// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Server.Maps;

[ImplicitDataDefinitionForInheritors]
public abstract partial class GameMapCondition
{
    [DataField("inverted")]
    public bool Inverted { get; private set; }
    public abstract bool Check(GameMapPrototype map);
}