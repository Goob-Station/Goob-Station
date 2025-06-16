using Lidgren.Network;
using static Content.Goobstation.Server.Voice.VoiceChatServerManager;

namespace Content.Goobstation.Server.Voice;

/// <summary>
/// Interface for the server-side voice chat manager.
/// </summary>
public interface IVoiceChatServerManager : IDisposable
{
    void Update();
    Dictionary<NetConnection, VoiceClientData> Clients { get; }
}
