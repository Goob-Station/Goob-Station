// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Server.Holiday.Interfaces;
using JetBrains.Annotations;

namespace Content.Server.Holiday.Greet
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class Custom : IHolidayGreet
    {
        [DataField("text")] private string _greet = string.Empty;

        public string Greet(HolidayPrototype holiday)
        {
            return Loc.GetString(_greet);
        }
    }
}
