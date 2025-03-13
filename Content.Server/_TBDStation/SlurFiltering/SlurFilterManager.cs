
using System.Text.RegularExpressions;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Content.Server.Chat.Managers;

namespace Content.Server._TBDStation.SlurFilter
{
    public sealed class SlurFilterManager : IPostInjectInit
    {
        [Dependency] private readonly IChatManager _chatManager = default!;

        private static readonly string[] Words = new string[] { "molest", "cum" };
        private static readonly string Pattern = string.Join("|", Words);
        // private static readonly string Pattern = @""
        private static readonly Regex _slurRegex = new Regex(Pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public void PostInject()
        {
        }
        public bool ContainsSlur(ICommonSession player, string message)
        {
            bool containsSlur = _slurRegex.IsMatch(message);
            if (containsSlur)
            {
                var feedback = Loc.GetString("server-slur-detected-warning");
                _chatManager.DispatchServerMessage(player, feedback);
            }
            return containsSlur;
        }

        internal bool ContainsSlur(string message)
        {
            bool containsSlur = _slurRegex.IsMatch(message);
            return containsSlur;
        }
    }
}
