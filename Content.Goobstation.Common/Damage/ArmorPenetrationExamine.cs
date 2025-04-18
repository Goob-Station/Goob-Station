using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Damage;

public sealed class ArmorPenetrationExamine
{
    public static FormattedMessage ArmorPenetrationExamineText(int? ap)
    {
        if (ap is null or 0)
            return [];

        var msg = new FormattedMessage();
        msg.AddText("\n" + Loc.GetString("armor-piercing-examine-start"));
        msg.PushColor(ap < 0 ? Color.Blue : Color.Red);
        msg.AddText(" " +  Math.Abs((int)ap) + "% " + (ap < 0 ? Loc.GetString("worse") : Loc.GetString("better")) + " ");
        msg.Pop();
        msg.AddText(Loc.GetString("armor-piercing-examine-end"));
        return msg;
    }
}
