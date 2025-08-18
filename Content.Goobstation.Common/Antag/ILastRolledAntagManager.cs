// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;

namespace Content.Goobstation.Common.Antag;

public interface ILastRolledAntagManager
{
    public void Initialize();

    /// <summary>
    /// Saves last rolled values to the database before allowing the server to shutdown.
    /// </summary>
    public void Shutdown();

    /// <summary>
    /// Sets a player's last rolled antag time.
    /// </summary>
    public TimeSpan SetLastRolled(NetUserId userId, TimeSpan to);

    /// <summary>
    /// Gets a player's last rolled antag time.
    /// </summary>
    public TimeSpan GetLastRolled(NetUserId userId);
}
