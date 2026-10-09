using System.Linq;
using Content.Shared.Chat;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.Chat.Widgets;

public partial class ChatBox
{
    private readonly IGameTiming _timing = IoCManager.Resolve<IGameTiming>();
    private GameTick _createdTick = IoCManager.Resolve<IGameTiming>().CurTick;

    private void UpdateCoalescence(bool value)
    {
        _coalescence = value;
        Repopulate();
    }

    public void Repopulate()
    {
        ClearContents();
        AddHistory();
    }

    private void OnChannelFilter(ChatChannel channel, bool active)
    {
        ClearContents();
        AddHistory();

        if (active)
        {
            _controller.ClearUnfilteredUnreads(channel);
        }
    }

    private void ClearContents()
    {
        Contents.Clear();

        foreach (var child in Contents.Children.ToArray())
        {
            if (child.Name != "_v_scroll")
            {
                Contents.RemoveChild(child);
            }
        }
    }

    private void AddHistory()
    {
        if (_timing.CurTick < _createdTick)
            _createdTick = GameTick.Zero;

        foreach (var (tick, message) in _controller.History)
        {
            if (tick < _createdTick)
                continue;

            OnMessageAdded(message);
        }
    }
}
