using System.Linq;
using Content.Goobstation.Common.VoxAudio;
using Content.Goobstation.Server.VoxAudio;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Goobstation.Server.Commands;

[ToolshedCommand, AdminCommand(AdminFlags.Fun)] // sorry trialmin
public sealed class VoxSayCommand : ToolshedCommand
{
    [CommandImplementation("global")]
    public void VoxSayGlobal(string toSay, string voiceProtoIds)
        => EntityManager.System<VoxAudioSystem>()
            .Play(toSay, [.. voiceProtoIds
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => new ProtoId<VoxVoicePrototype>(id))]);
    // Good. Keep smiling

    [CommandImplementation("entity")]
    public void VoxSayEntity([PipedArgument] EntityUid uid, string toSay, string voiceProtoIds)
        => EntityManager.System<VoxAudioSystem>()
            .Play(toSay, [.. voiceProtoIds
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => new ProtoId<VoxVoicePrototype>(id))], 0, 0, uid);
}