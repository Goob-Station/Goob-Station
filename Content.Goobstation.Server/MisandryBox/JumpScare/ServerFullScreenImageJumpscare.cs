using Content.Goobstation.Shared.MisandryBox.JumpScare;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.MisandryBox.JumpScare;

public sealed class ServerFullScreenImageJumpscare : IFullScreenImageJumpscare, IPostInjectInit
{
    [Dependency] private readonly INetManager _netManager = default!;

    public void Jumpscare(SpriteSpecifier.Texture image, ICommonSession? session)
    {
        DebugTools.Assert(session is not null);
        Jumpscare(session.Channel, image.TexturePath);
    }

    private void Jumpscare(INetChannel channel, ResPath imagepath)
    {
        var msg = new JumpscareMessage()
        {
            ImagePath = imagepath.CanonPath,
        };

        _netManager.ServerSendMessage(msg, channel);
    }

    public void PostInject()
    {
        RegisterNetMessages();
    }

    private void RegisterNetMessages()
    {
        _netManager.RegisterNetMessage<JumpscareMessage>();
    }
}
