
using System.Text.RegularExpressions;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Content.Server.Chat.Managers;

namespace Content.Server._TBDStation.SlurFilter
{
    public sealed class SlurFilterSystem : EntitySystem
    {
        [Dependency] private readonly SlurFilterManager _chatManager = default!;
        [Dependency] private readonly MetaDataSystem _metaSystem = default!;
        public override void Initialize()
        {
            // SubscribeLocalEvent<ActorComponent, EntityRenamedEvent>(OnPlayerRenamed);
            SubscribeLocalEvent<EntityRenamedEvent>(OnRename);
        }

        int i = 0;
        private void OnRename(ref EntityRenamedEvent ev)
        {
            if (ev.NewName == null || ev.NewName == "" || ev.NewName == " ")
            {
                return;
            }
            i += 1;
            if (_chatManager.ContainsSlur(ev.NewName))
            {
                int j = 0;
                j += 1; // TODO: Log this change
                _metaSystem.SetEntityName(ev.Uid, ev.OldName);
            }
        }
    }
}
