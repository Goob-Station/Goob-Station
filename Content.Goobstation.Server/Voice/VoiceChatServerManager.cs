// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Concentus;
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
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Voice;

/// <summary>
/// Server-side manager for voice chat functionality.
/// Handles Lidgren UDP connections and decodes Opus audio.
/// </summary>
public sealed class VoiceChatServerManager : IVoiceChatServerManager, IPostInjectInit
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private ISawmill _sawmill = default!;

    private NetServer? _server;
    private bool _running;
    private int _port;
    private string _appIdentifier = "SS14VoiceChat";

    public Dictionary<NetConnection, VoiceClientData> Clients { get; } = new();

    private const int SampleRate = 48000;
    private const int Channels = 1; // Mono
    private const int FrameSizeMs = 20;
    private const int FrameSamplesPerChannel = SampleRate / 1000 * FrameSizeMs; // 960
    private const int BytesPerSample = 2; // 16-bit audio

    public void PostInject()
    {
        _sawmill = Logger.GetSawmill("voiceserver");

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
        _port = port;
        if (_running)
        {
            _sawmill.Info("Voice chat port changed. Restarting server...");
            StopServer();
            StartServer();
        }
    }

    /// <summary>
    /// Start the Lidgren UDP server for voice chat.
    /// </summary>
    private void StartServer()
    {
        if (_running) return;

        var config = new NetPeerConfiguration(_appIdentifier)
        {
            Port = _port,
            MaximumConnections = _cfg.GetCVar(CCVars.SoftMaxPlayers),
            ConnectionTimeout = 30.0f
        };
        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        config.EnableMessageType(NetIncomingMessageType.StatusChanged);
        config.EnableMessageType(NetIncomingMessageType.Data); // For voice data
        config.EnableMessageType(NetIncomingMessageType.WarningMessage);
        config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
        config.EnableMessageType(NetIncomingMessageType.DebugMessage);

        try
        {
            _server = new NetServer(config);
            _server.Start();
            _running = true;
            _sawmill.Info($"Voice server started on port {_port}");
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to start voice server: {e.Message}");
            _running = false;
            _server = null;
        }
    }

    /// <summary>
    /// Stop the Lidgren UDP server for voice chat.
    /// </summary>
    private void StopServer()
    {
        if (!_running || _server == null) return;

        _sawmill.Info("Stopping voice server...");

        var connectionsToDisconnect = new List<NetConnection>(Clients.Keys);
        foreach (var conn in connectionsToDisconnect)
        {
            conn.Disconnect("Server shutting down.");
        }
        Clients.Clear();

        _server.Shutdown("Server shutting down.");
        _server = null;
        _running = false;
        _sawmill.Info("Voice server stopped.");
    }

    public void Update()
    {
        if (!_running || _server == null) return;

        NetIncomingMessage? msg;
        while ((msg = _server.ReadMessage()) != null)
        {
            try
            {
                ProcessMessage(msg);
            }
            catch (Exception e)
            {
                _sawmill.Error($"Error processing Lidgren message: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                _server.Recycle(msg);
            }
        }
    }

    /// <summary>
    /// Process incoming Lidgren messages.
    /// </summary>
    private void ProcessMessage(NetIncomingMessage msg)
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

                _sawmill.Debug($"Voice client {connection.RemoteEndPoint.Address} status changed: {status}. Reason: {reason}");

                if (status == NetConnectionStatus.Connected)
                {
                    HandleClientConnected(connection);
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
                _sawmill.Warning($"Lidgren Warning: {msg.ReadString()} from {msg.SenderEndPoint?.Address}");
                break;

            case NetIncomingMessageType.ErrorMessage:
                _sawmill.Error($"Lidgren Error: {msg.ReadString()} from {msg.SenderEndPoint?.Address}");
                break;

            case NetIncomingMessageType.DebugMessage:
                _sawmill.Debug($"Lidgren Debug: {msg.ReadString()} from {msg.SenderEndPoint?.Address}");
                break;

            default:
                _sawmill.Debug($"Unhandled Lidgren message type: {msg.MessageType} from {msg.SenderEndPoint?.Address}");
                break;
        }
    }

    /// <summary>
    /// Handle connection approval requests.
    /// </summary>
    private void HandleConnectionApproval(NetIncomingMessage msg)
    {
        ICommonSession? matchedSession = null;

        foreach (var commonSession in _playerManager.Sessions)
        {
            if (commonSession is ICommonSession session &&
                session.Channel.RemoteEndPoint.Address.Equals(msg.SenderEndPoint?.Address) &&
                session.Status == SessionStatus.InGame)
            {
                matchedSession = session;
                break;
            }
        }

        if (matchedSession != null)
        {
            _sawmill.Debug($"Approving voice connection from {msg.SenderEndPoint?.Address} for player {matchedSession.Name}");
            msg.SenderConnection?.Approve();
        }
        else
        {
            _sawmill.Warning($"Denying voice connection from {msg.SenderEndPoint?.Address}: No matching active player session found or player not in game.");
            msg.SenderConnection?.Deny("No matching player session.");
        }
    }

    /// <summary>
    /// Handle client connection.
    /// </summary>
    private void HandleClientConnected(NetConnection connection)
    {
        if (Clients.ContainsKey(connection))
        {
            _sawmill.Warning($"Received Connected status for already tracked client {connection.RemoteEndPoint.Address}. Ignoring.");
            return;
        }

        ICommonSession? session = null;

        foreach (var commonSession in _playerManager.Sessions)
        {
            if (commonSession is ICommonSession playerSession &&
                playerSession.Channel.RemoteEndPoint.Address.Equals(connection.RemoteEndPoint.Address))
            {
                session = playerSession;
                break;
            }
        }

        if (session == null)
        {
            _sawmill.Error($"Voice client {connection.RemoteEndPoint.Address} connected, but corresponding session object is null. Disconnecting.");
            connection.Disconnect("Failed to find player session object. Are you in-game?");
            return;
        }

        _sawmill.Debug($"Found session for {connection.RemoteEndPoint.Address}: Player={session.Name}, Status={session.Status}, AttachedEntity={session.AttachedEntity}");

        if (session.AttachedEntity is not { Valid: true } entityUid)
        {
            _sawmill.Error($"Voice client {connection.RemoteEndPoint.Address} connected for player {session.Name}, but attached entity is null or invalid ({session.AttachedEntity}). Disconnecting.");
            connection.Disconnect("Failed to find valid player entity. Are you in round?");
            return;
        }

        _sawmill.Info($"Successfully associated voice client {connection.RemoteEndPoint.Address} with player {session.Name} (Entity: {entityUid}). Adding to tracked clients.");

        var clientData = new VoiceClientData(connection, entityUid, SampleRate, Channels);

        if (!Clients.TryAdd(connection, clientData))
        {
            _sawmill.Warning($"Failed to add voice client {connection.RemoteEndPoint.Address} to dictionary, already exists? Disconnecting.");
            clientData.Dispose();
            connection.Disconnect("Internal server error adding client.");
        }
    }

    /// <summary>
    /// Handle client disconnection.
    /// </summary>
    private void HandleClientDisconnected(NetConnection connection, string reason)
    {
        if (Clients.TryGetValue(connection, out var clientData))
        {
            _sawmill.Info($"Voice client {connection.RemoteEndPoint.Address} (Player Entity: {clientData.PlayerEntity}) disconnected. Reason: {reason}");
            clientData.Dispose();
            Clients.Remove(connection);
        }
        else
        {
            _sawmill.Debug($"Received Disconnected status for untracked or already removed client {connection.RemoteEndPoint.Address}. Reason: {reason}");
        }
    }

    /// <summary>
    /// Handle incoming voice data.
    /// </summary>
    private void HandleVoiceData(NetIncomingMessage msg)
    {
        if (msg.SenderConnection == null)
        {
            _sawmill.Warning($"Received voice data message with null SenderConnection. Discarding.");
            return;
        }

        if (!Clients.TryGetValue(msg.SenderConnection, out var clientData))
        {
            _sawmill.Warning($"Received voice data from unknown connection: {msg.SenderConnection.RemoteEndPoint.Address}. Discarding.");
            return;
        }

        if (!_entityManager.TryGetComponent<TransformComponent>(clientData.PlayerEntity, out var transform) ||
            transform.MapID == MapId.Nullspace ||
            !_entityManager.EntityExists(clientData.PlayerEntity))
        {
            _sawmill.Debug($"Voice data received for invalid, non-existent, or nullspace entity {clientData.PlayerEntity}. Discarding.");
            return;
        }

        var startTime = _gameTiming.RealTime;
        TimeSpan decodeTime, pvsTime, raiseTime;

        try
        {
            int frameSize = FrameSamplesPerChannel;
            short[] pcmBuffer = clientData.GetPcmBuffer();
            int dataLength = msg.LengthBytes;

            if (clientData.Decoder == null)
            {
                _sawmill.Error($"Decoder is null for client {clientData.Connection.RemoteEndPoint.Address}. Cannot process audio.");
                return;
            }

            byte[] opusDataBuffer = clientData.GetOpusBuffer(dataLength);

            if (msg.LengthBytes < dataLength)
            {
                _sawmill.Warning($"Voice data message length ({msg.LengthBytes}) is less than expected ({dataLength}). Discarding.");
                return;
            }

            msg.ReadBytes(opusDataBuffer, 0, dataLength);

            var decodeStartTime = _gameTiming.RealTime;

            var opusSpan = opusDataBuffer.AsSpan(0, dataLength);
            var pcmSpan = pcmBuffer.AsSpan();
            int decodedSamples = clientData.Decoder.Decode(opusSpan, pcmSpan, frameSize, false);
            decodeTime = _gameTiming.RealTime - decodeStartTime;

            if (decodedSamples > 0)
            {
                int byteCount = decodedSamples * Channels * BytesPerSample;
                byte[] pcmBytes = clientData.GetByteBuffer(byteCount);
                Buffer.BlockCopy(pcmBuffer, 0, pcmBytes, 0, byteCount);

                var pvsStartTime = _gameTiming.RealTime;
                var filter = Filter.Pvs(clientData.PlayerEntity);
                pvsTime = _gameTiming.RealTime - pvsStartTime;

                var raiseStartTime = _gameTiming.RealTime;

                // Send as NetMessage instead of EntityEvent
                var netMsg = new MsgVoiceChat
                {
                    PcmData = pcmBytes,
                    SourceEntity = _entityManager.GetNetEntity(clientData.PlayerEntity)
                };

                var channels = filter.Recipients
                    .Where(s => s.Channel != null)
                    .Select(s => s.Channel)
                    .ToList();

                _netManager.ServerSendToMany(netMsg, channels);
                raiseTime = _gameTiming.RealTime - raiseStartTime;

                var totalTime = _gameTiming.RealTime - startTime;
                _sawmill.Debug(
                    $"VoiceData Handled: Src={clientData.PlayerEntity}, Bytes={byteCount}, " +
                    $"Decode={decodeTime.TotalMilliseconds:F1}ms, " +
                    $"PVS={pvsTime.TotalMilliseconds:F1}ms, " +
                    $"Raise={raiseTime.TotalMilliseconds:F1}ms, " +
                    $"Total={totalTime.TotalMilliseconds:F1}ms");
            }
            else
            {
                _sawmill.Warning($"Opus decoding failed or produced 0 samples for client {clientData.Connection.RemoteEndPoint.Address}. Code: {decodedSamples}");
            }
        }
        catch (Exception e)
        {
            _sawmill.Error($"Error processing voice data from {msg.SenderConnection.RemoteEndPoint.Address}: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Handle player status changes.
    /// </summary>
    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Disconnected || e.OldStatus == SessionStatus.InGame && e.NewStatus != SessionStatus.InGame)
        {
            NetConnection? connectionToDrop = null;
            VoiceClientData? dataToDrop = null;

            if (e.Session is not ICommonSession playerSession)
                return;

            var playerEndpoint = playerSession.Channel.RemoteEndPoint.Address;

            foreach (var conn in Clients.Keys.ToList())
            {
                if (conn.RemoteEndPoint.Address.Equals(playerEndpoint))
                {
                    connectionToDrop = conn;
                    dataToDrop = Clients[conn];
                    break;
                }
            }

            if (connectionToDrop != null && dataToDrop != null)
            {
                _sawmill.Info($"Player {e.Session.Name} status changed from {e.OldStatus} to {e.NewStatus}. Disconnecting associated voice client {connectionToDrop.RemoteEndPoint.Address} (Entity: {dataToDrop.PlayerEntity}).");
                connectionToDrop.Disconnect($"Player session ended (Status: {e.NewStatus}).");
            }
        }
    }

    /// <summary>
    /// Dispose the voice chat server manager.
    /// </summary>
    public void Dispose()
    {
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatEnabled, OnVoiceChatEnabledChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatPort, OnVoiceChatPortChanged);
        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;

        StopServer();

        _sawmill.Info("VoiceChatServerManager disposed");
    }

    /// <summary>
    /// Helper class to store state associated with a connected voice client.
    /// </summary>
    public sealed class VoiceClientData : IDisposable
    {
        public NetConnection Connection { get; }
        public EntityUid PlayerEntity { get; set; }
        public OpusDecoder? Decoder { get; private set; }

        private short[] _pcmBuffer;
        private byte[] _opusReadBuffer;
        private byte[] _byteBuffer;

        private static readonly ISawmill _sawmill = Logger.GetSawmill("voiceserver");

        public VoiceClientData(NetConnection connection, EntityUid playerEntity, int sampleRate, int channels)
        {
            Connection = connection;
            PlayerEntity = playerEntity;

            _pcmBuffer = new short[FrameSamplesPerChannel * channels];
            _opusReadBuffer = new byte[4000];
            _byteBuffer = new byte[FrameSamplesPerChannel * channels * BytesPerSample];

            try
            {
                Decoder = (OpusDecoder?) OpusCodecFactory.CreateDecoder(sampleRate, channels);
            }
            catch (Exception e)
            {
                _sawmill.Error($"Failed to create OpusDecoder for {connection.RemoteEndPoint.Address}: {e.Message}");
                Decoder = null;
            }
        }

        public short[] GetPcmBuffer() => _pcmBuffer;

        public byte[] GetOpusBuffer(int requiredSize)
        {
            if (_opusReadBuffer.Length < requiredSize)
            {
                _sawmill.Warning($"Resizing Opus read buffer from {_opusReadBuffer.Length} to {requiredSize}. This might indicate unusually large voice packets.");
                _opusReadBuffer = new byte[requiredSize];
            }

            return _opusReadBuffer;
        }

        public byte[] GetByteBuffer(int requiredSize)
        {
            if (_byteBuffer.Length < requiredSize)
            {
                _byteBuffer = new byte[requiredSize];
            }
            return _byteBuffer;
        }

        public void Dispose()
        {
            Decoder = null;
            _sawmill.Debug($"Disposed VoiceClientData for {Connection.RemoteEndPoint.Address}");
        }
    }
}
