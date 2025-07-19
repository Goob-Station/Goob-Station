namespace Content.Pirate.Common._EinsteinEngines.Chat
{
    public interface ITelepathicChatSystem
    {
        /// <summary>
        /// Надсилає телепатичне повідомлення всім гравцям-псіонікам.
        /// </summary>
        /// <param name="sender">Відправник повідомлення</param>
        /// <param name="message">Текст повідомлення</param>
        /// <param name="senderName">Ім'я відправника</param>
        /// <param name="hideChat">Чи приховувати повідомлення у звичайному чаті</param>
        void SendTelepathicChat(EntityUid sender, string message, string senderName, bool hideChat = false);
    }
}
