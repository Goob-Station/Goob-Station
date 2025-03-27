using Content.Client.Items;
using Content.Shared._White.Blink;

namespace Content.Client._White.Blink;

public sealed class BlinkSystem : SharedBlinkSystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<BlinkComponent>(ent => new BlinkStatusControl(ent));
    }
}
