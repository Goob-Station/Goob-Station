// SPDX-FileCopyrightText: 2020 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Atmos
{
    public sealed class AtmosCommandUtils
    {
        /// <summary>
        /// Gas ID parser for atmospherics commands.
        /// This is so there's a central place for this logic for if the Gas enum gets removed.
        /// </summary>
        public static bool TryParseGasID(string str, out int x)
        {
            x = -1;
            if (Enum.TryParse<Gas>(str, true, out var gas))
            {
                x = (int) gas;
            }
            else
            {
                if (!int.TryParse(str, out x))
                    return false;
            }
            return ((x >= 0) && (x < Atmospherics.TotalNumberOfGases));
        }
    }
}