// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using Robust.Client;

namespace Content.Goobstation.Client;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        ContentStart.Start(args);
    }
}
