using Content.Shared.Chat;

namespace Content.Client.Chat.Managers
{
    public interface IChatManager : ISharedChatManager
    {
        void Initialize(); // Goobstation - Starlight collective mind port

        public void SendMessage(string text, ChatSelectChannel channel);

        /// <summary>
        ///     Will refresh perms.
        /// </summary>
        event Action PermissionsUpdated;
        public void UpdatePermissions();
    }
}
