using System.Linq;
using Concentus.Structs;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Shared.CCVar;
using Lidgren.Network;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Threading.Tasks;
using System.Net;
using Concentus;

namespace Content.Goobstation.Server.Voice;
public sealed class VoiceChatServerManager : IVoiceChatServerManager, IPostInjectInit, IDisposable
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    private ISawmill _sawmill = default!;

    private NetServer? _server;
    private bool _running;
    private int _port = GoobCVars.VoiceChatPort.DefaultValue;
    private string _appIdentifier = "SS14VoiceChat";

    private readonly Dictionary<NetConnection, VoiceClientData> _clients = new();

    private const int SampleRate = 48000;
    private const int Channels = 1;
    private const int FrameSizeMs = 20;
    private const int FrameSamplesPerChannel = SampleRate / 1000 * FrameSizeMs;
    private const int BytesPerSample = 2;

    public void PostInject()
    {
        _sawmill = Logger.GetSawmill("voice.server");

        _cfg.OnValueChanged(GoobCVars.VoiceChatEnabled, OnVoiceChatEnabledChanged, true);
        _cfg.OnValueChanged(GoobCVars.VoiceChatPort, OnVoiceChatPortChanged, true);

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        _netManager.RegisterNetMessage<MsgVoiceChat>();

        _sawmill.Info("VoiceChatServerManager initialized");
    }

    private void OnVoiceChatEnabledChanged(bool enabled)
    {
        if (enabled && !_running)
            StartServer();
        else if (!enabled && _running)
            StopServer();
    }

    private void OnVoiceChatPortChanged(int port)
    {
        if (_port == port)
            return;

        _port = port;
        if (_running)
        {
            _sawmill.Info("Voice chat port changed. Restarting server...");
            StopServer();
            StartServer();
        }
    }

    private void StartServer()
    {
        if (_running) return;

        if (_port <= 0 || _port > 65535)
        {
            _sawmill.Error($"Invalid voice chat port configured: {_port}. Voice server not started.");
            return;
        }

        var config = new NetPeerConfiguration(_appIdentifier)
        {
            Port = _port,
            MaximumConnections = _cfg.GetCVar(CCVars.SoftMaxPlayers),
            ConnectionTimeout = 20.0f,
            UseMessageRecycling = true,
        };
        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        config.EnableMessageType(NetIncomingMessageType.StatusChanged);
        config.EnableMessageType(NetIncomingMessageType.Data);
        config.EnableMessageType(NetIncomingMessageType.WarningMessage);
        config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
        config.EnableMessageType(NetIncomingMessageType.DebugMessage);

        try
        {
            _server = new NetServer(config);
            _server.Start();
            _running = true;
            _sawmill.Info($"Voice server started on port {_port}");
            Task.Run(NetworkLoop);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to start voice server on port {_port}: {e.Message}");
            _running = false;
            _server = null;
        }
    }

    private void StopServer()
    {
        if (!_running || _server == null) return;

        _sawmill.Info("Stopping voice server...");
        _running = false;

        var connectionsToDisconnect = _clients.Keys.ToList();
        _clients.Clear();
        foreach (var conn in connectionsToDisconnect)
        {
            try { conn.Disconnect("Server shutting down."); } catch { /* Ignore */ }
        }

        _server.Shutdown("Server shutting down.");
        _server = null;
        _sawmill.Info("Voice server stopped.");
    }

    private async Task NetworkLoop()
    {
        _sawmill.Debug("Voice server network loop started.");
        while (_running && _server != null && _server.Status == NetPeerStatus.Running)
        {
            NetIncomingMessage? msg;
            while ((msg = _server.ReadMessage()) != null)
            {
                try
                {
                    ProcessMessageInternal(msg);
                }
                catch (Exception e)
                {
                    _sawmill.Error($"Error processing Lidgren message in network loop: {e.Message}\n{e.StackTrace}");
                }
                finally
                {
                    _server.Recycle(msg);
                }
            }
            await Task.Yield();
        }
        _sawmill.Debug("Voice server network loop stopped.");
    }

    private void ProcessMessageInternal(NetIncomingMessage msg)
    {
        switch (msg.MessageType)
        {
            case NetIncomingMessageType.ConnectionApproval:
                HandleConnectionApproval(msg);
                break;

            case NetIncomingMessageType.StatusChanged:
                var status = (NetConnectionStatus) msg.ReadByte();
                var reason = msg.ReadString();
                var connection = msg.SenderConnection;

                if (connection == null)
                {
                    _sawmill.Warning($"Received StatusChanged message with null SenderConnection. Status: {status}, Reason: {reason}");
                    break;
                }

                _sawmill.Debug($"Voice client {connection.RemoteEndPoint} status changed: {status}. Reason: {reason}");

                if (status == NetConnectionStatus.Connected)
                {
                    AssociateConnectionToPlayer(connection);
                }
                else if (status == NetConnectionStatus.Disconnected)
                {
                    HandleClientDisconnected(connection, reason);
                }
                break;

            case NetIncomingMessageType.Data:
                HandleVoiceData(msg);
                break;

            case NetIncomingMessageType.WarningMessage:
                _sawmill.Warning($"Lidgren Warning: {msg.ReadString()} from {msg.SenderEndPoint}");
                break;
            case NetIncomingMessageType.ErrorMessage:
                _sawmill.Error($"Lidgren Error: {msg.ReadString()} from {msg.SenderEndPoint}");
                break;
            case NetIncomingMessageType.DebugMessage:
                _sawmill.Debug($"Lidgren Debug: {msg.ReadString()} from {msg.SenderEndPoint}");
                break;
            default:
                _sawmill.Debug($"Unhandled Lidgren message type: {msg.MessageType} from {msg.SenderEndPoint}");
                break;
        }
    }

    private void HandleConnectionApproval(NetIncomingMessage msg)
    {
        _sawmill.Debug($"Approving voice connection from {msg.SenderEndPoint}");
        msg.SenderConnection?.Approve();
    }

    private void HandleClientDisconnected(NetConnection connection, string reason)
    {
        if (_clients.Remove(connection, out var clientData))
        {
            _sawmill.Info($"Voice client {connection.RemoteEndPoint} (Player: {clientData.Session.Name}) disconnected. Reason: {reason}");
            clientData.Dispose();
        }
        else
        {
            _sawmill.Debug($"Received Disconnected status for untracked client {connection.RemoteEndPoint}. Reason: {reason}");
        }
    }

    private ICommonSession? FindPlayerSessionByEndpoint(IPEndPoint endpoint)
    {
        foreach (var session in _playerManager.Sessions)
            if (session.Channel.RemoteEndPoint.Address.Equals(endpoint.Address))
                return session;
        return null;
    }

    private void AssociateConnectionToPlayer(NetConnection connection)
    {
        if (_clients.ContainsKey(connection)) return;

        var session = FindPlayerSessionByEndpoint(connection.RemoteEndPoint);

        if (session != null && session.Status == SessionStatus.InGame && session.AttachedEntity.HasValue)
        {
            var entityUid = session.AttachedEntity.Value;
            if (!_entityManager.EntityExists(entityUid))
            {
                _sawmill.Warning($"Voice connection {connection.RemoteEndPoint} matched player {session.Name}, but attached entity {entityUid} does not exist. Association deferred.");
                return;
            }

            var clientData = new VoiceClientData(connection, session, entityUid, SampleRate, Channels);
            if (clientData.Decoder == null)
            {
                _sawmill.Error($"Failed to create Opus decoder for player {session.Name}. Disconnecting voice connection {connection.RemoteEndPoint}.");
                clientData.Dispose();
                connection.Disconnect("Server error: Could not initialize voice decoder.");
                return;
            }

            if (_clients.TryAdd(connection, clientData))
            {
                _sawmill.Info($"Associated player {session.Name} (Entity: {entityUid}) with voice connection {connection.RemoteEndPoint}");
            }
            else
            {
                _sawmill.Warning($"Failed to add voice connection {connection.RemoteEndPoint} to dictionary for player {session.Name}, already exists?");
                clientData.Dispose();
            }
        }
        else
        {
            _sawmill.Warning($"Voice connection {connection.RemoteEndPoint} established, but no matching *InGame* player session with valid entity found. Association deferred.");
        }
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        var session = e.Session;

        if (e.NewStatus == SessionStatus.InGame)
        {
            var voiceConnection = FindVoiceConnectionForSession(session);
            if (voiceConnection != null && !_clients.ContainsKey(voiceConnection))
            {
                AssociateConnectionToPlayer(voiceConnection);
            }
        }
        else if (e.OldStatus == SessionStatus.InGame && e.NewStatus != SessionStatus.InGame)
        {
            NetConnection? connToRemove = null;
            foreach (var kvp in _clients)
            {
                if (kvp.Value.Session == session)
                {
                    connToRemove = kvp.Key;
                    break;
                }
            }
            if (connToRemove != null)
            {
                _sawmill.Info($"Player {session.Name} left game (Status: {e.NewStatus}). Disconnecting associated voice client {connToRemove.RemoteEndPoint}.");
                HandleClientDisconnected(connToRemove, $"Player left game (Status: {e.NewStatus}).");
                try { connToRemove.Disconnect($"Player left game (Status: {e.NewStatus})."); } catch {/* Ignore */}
            }
        }
    }

    private NetConnection? FindVoiceConnectionForSession(ICommonSession session)
    {
        var sessionEndpoint = session.Channel.RemoteEndPoint;
        if (_server == null) return null;

        foreach (var connection in _server.Connections)
        {
            if (connection.RemoteEndPoint.Address.Equals(sessionEndpoint.Address))
            {
                return connection;
            }
        }
        return null;
    }

    private void HandleVoiceData(NetIncomingMessage msg)
    {
        var senderConnection = msg.SenderConnection;
        if (senderConnection == null) return;

        if (!_clients.TryGetValue(senderConnection, out var clientData))
        {
            _sawmill.Warning($"Received voice data from unassociated connection: {senderConnection.RemoteEndPoint}. Discarding.");
            return;
        }

        if (!_entityManager.TryGetComponent<TransformComponent>(clientData.PlayerEntity, out var transform) ||
            transform.MapID == MapId.Nullspace ||
            !_entityManager.EntityExists(clientData.PlayerEntity))
        {
            _sawmill.Debug($"Voice data received for invalid/nullspace entity {clientData.PlayerEntity}. Discarding.");
            return;
        }

        var dataLength = msg.LengthBytes;
        if (dataLength <= 0) return;

        byte[] opusDataBuffer = clientData.GetOpusBuffer(dataLength);

        if (!msg.TryReadBytes(opusDataBuffer.AsSpan(0, dataLength)))
        {
            _sawmill.Warning($"Failed to read {dataLength} bytes of voice data from {clientData.Connection.RemoteEndPoint}.");
            return;
        }

        if (clientData.Decoder == null)
        {
            _sawmill.Error($"Decoder is null for client {clientData.Connection.RemoteEndPoint}. Cannot process audio.");
            return;
        }

        short[] pcmBuffer = clientData.GetPcmBuffer();
        var opusSpan = opusDataBuffer.AsSpan(0, dataLength);
        var pcmSpan = pcmBuffer.AsSpan();
        int decodedSamples = 0;
        try
        {
            decodedSamples = clientData.Decoder.Decode(opusSpan, pcmSpan, FrameSamplesPerChannel, false);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Opus decoding error for {clientData.Connection.RemoteEndPoint}: {e.Message}");
            return;
        }

        if (decodedSamples <= 0)
        {
            _sawmill.Warning($"Opus decoding failed or produced 0 samples for client {clientData.Connection.RemoteEndPoint}. Code: {decodedSamples}");
            return;
        }

        int byteCount = decodedSamples * Channels * BytesPerSample;
        byte[] pcmBytesToSend = clientData.GetByteBuffer(byteCount);
        Buffer.BlockCopy(pcmBuffer, 0, pcmBytesToSend, 0, byteCount);

        var filter = Filter.Pvs(clientData.PlayerEntity, entityManager: _entityManager);

        var netMsg = new MsgVoiceChat
        {
            PcmData = pcmBytesToSend.AsSpan(0, byteCount).ToArray(),
            SourceEntity = _entityManager.GetNetEntity(clientData.PlayerEntity)
        };

        var recipients = filter.Recipients
            .Where(s => s != null && s.AttachedEntity != clientData.PlayerEntity)
            .Select(s => s.Channel)
            .Where(c => c != null)
            .Cast<INetChannel>()
            .ToList();

        if (recipients.Count > 0)
        {
            _netManager.ServerSendToMany(netMsg, recipients);
            _sawmill.Debug($"Sent voice PCM data ({byteCount} bytes) from {clientData.PlayerEntity} to {recipients.Count} clients.");
        }
    }

    public void Dispose()
    {
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatEnabled, OnVoiceChatEnabledChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatPort, OnVoiceChatPortChanged);
        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
        StopServer();
        _sawmill.Info("VoiceChatServerManager disposed");
    }

    private sealed class VoiceClientData : IDisposable
    {
        public NetConnection Connection { get; }
        public ICommonSession Session { get; }
        public EntityUid PlayerEntity { get; }
        public OpusDecoder? Decoder { get; private set; }

        private short[] _pcmBuffer;
        private byte[] _opusReadBuffer;
        private byte[] _byteBuffer;

        private static readonly ISawmill _sawmill = Logger.GetSawmill("voice.server.clientdata");

        public VoiceClientData(NetConnection connection, ICommonSession session, EntityUid playerEntity, int sampleRate, int channels)
        {
            Connection = connection;
            Session = session;
            PlayerEntity = playerEntity;

            _pcmBuffer = new short[FrameSamplesPerChannel * channels];
            _opusReadBuffer = new byte[4096];
            _byteBuffer = new byte[FrameSamplesPerChannel * channels * BytesPerSample];

            try
            {
                Decoder = (OpusDecoder?) OpusCodecFactory.CreateDecoder(sampleRate, channels);
            }
            catch (Exception e)
            {
                _sawmill.Error($"Failed to create OpusDecoder for {connection.RemoteEndPoint}: {e.Message}");
                Decoder = null;
            }
        }

        public short[] GetPcmBuffer() => _pcmBuffer;

        public byte[] GetOpusBuffer(int requiredSize)
        {
            if (_opusReadBuffer.Length < requiredSize)
            {
                _sawmill.Warning($"Resizing Opus read buffer from {_opusReadBuffer.Length} to {requiredSize} for {Connection.RemoteEndPoint}.");
                Array.Resize(ref _opusReadBuffer, requiredSize);
            }
            return _opusReadBuffer;
        }

        public byte[] GetByteBuffer(int requiredSize)
        {
            if (_byteBuffer.Length < requiredSize)
            {
                Array.Resize(ref _byteBuffer, requiredSize);
            }
            return _byteBuffer;
        }

        public void Dispose()
        {
            Decoder = null;
            _sawmill.Debug($"Disposed VoiceClientData for {Connection.RemoteEndPoint}");
        }
    }
}
