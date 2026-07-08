// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Chat;

public interface ISharedChatManager
{
    void Initialize();
    void SendAdminAlert(string message);
    void SendAdminAlert(EntityUid player, string message);
}