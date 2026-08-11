using Content.Goobstation.Shared.JoinQueue;
using Robust.Client.Audio;
using Robust.Client.Console;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.JoinQueue;

public sealed class QueueState : State
{
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!;
    [Dependency] private readonly IClientConsoleHost _console = default!;


    private static readonly SoundPathSpecifier JoinSound = new(new ResPath("/Audio/Effects/newplayerping.ogg"));

    private QueueGui? _gui;


    protected override void Startup()
    {
        _gui = new QueueGui();
        _userInterface.StateRoot.AddChild(_gui);

        _gui.QuitPressed += OnQuitPressed;
    }

    protected override void Shutdown()
    {
        _gui!.QuitPressed -= OnQuitPressed;
        _gui.RemoveAllChildren();

        Ding();
    }


    public void OnQueueUpdate(QueueUpdateMessage msg)
    {
        _gui?.UpdateInfo(msg);
    }

    private void OnQuitPressed()
    {
        _console.ExecuteCommand("disconnect");
    }


    private void Ding()
    {
        if (IoCManager.Resolve<IEntityManager>().TrySystem<AudioSystem>(out var audio))
            audio.PlayGlobal(JoinSound, Filter.Local(), false);
    }
}
