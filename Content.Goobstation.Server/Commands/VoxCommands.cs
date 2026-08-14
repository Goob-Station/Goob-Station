using System.Linq;
using Content.Goobstation.Common.VoxAudio;
using Content.Goobstation.Server.VoxAudio;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Goobstation.Server.Commands;

[ToolshedCommand, AnyCommand]
public sealed class VoxCommand : ToolshedCommand
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [CommandImplementation("list")]
    public IEnumerable<string> VoxList(ProtoId<VoxVoicePrototype> voiceProto)
        => _proto.Index(voiceProto).Words.Select(x => x.Word);

    [CommandImplementation("validate")]
    public IEnumerable<string> VoxValidate(string toSay, ProtoId<VoxVoicePrototype> voiceProto)
        => EntityManager.System<VoxAudioSystem>()
            .GetPlaybackWordChain([_proto.Index(voiceProto)], toSay).Select(x => x.Word);
}