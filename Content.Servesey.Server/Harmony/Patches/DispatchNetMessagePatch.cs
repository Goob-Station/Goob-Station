using HarmonyLib;
using Lidgren.Network;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Network;
using Robust.Shared;
using SpaceWizards.Sodium;

namespace Content.Servesey.Server.Harmony.Patches;

/// <summary>
/// Equivalent to:
/// https://github.com/space-wizards/RobustToolbox/pull/6491
/// https://github.com/space-wizards/RobustToolbox/pull/6492
/// </summary>
[HarmonyPatch(typeof(NetManager), "DispatchNetMessage")]
public static class DispatchNetMessagePatch
{
    private static bool _logPacketIssues;
    private static IConfigurationManager? _cfg;
    private static ISawmill _sawmill = default!;

    public static void Initialize()
    {
        _cfg ??= IoCManager.Resolve<IConfigurationManager>();
        _logPacketIssues = _cfg.GetCVar(CVars.NetLogging);
        _cfg.OnValueChanged(CVars.NetLogging, value => _logPacketIssues = value);
        _sawmill = Logger.GetSawmill("net");
    }

    /// <summary>
    /// Catches SodiumException thrown by Decrypt, disconnects the sender, and suppresses the exception
    /// </summary>
    static Exception? Finalizer(Exception? __exception, ref bool __result, NetIncomingMessage msg)
    {
        if (__exception is SodiumException)
        {
            if (_logPacketIssues)
                _sawmill.Debug($"{msg.SenderConnection?.RemoteEndPoint}: Got a packet that fails to decrypt.");

            msg.SenderConnection?.Disconnect("Failed to decrypt packet.");
            __result = true;
            return null;
        }

        return __exception;
    }

}
