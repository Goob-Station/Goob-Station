// SPDX-License-Identifier: MIT

using Content.Shared.Humanoid.Markings;
using Content.Shared.Localizations;
using Content.Shared.Tag;
using Content.Shared.Whitelist;

namespace Content.Shared.IoC
{
    public static class SharedContentIoC
    {
        public static void Register()
        {
            IoCManager.Register<MarkingManager, MarkingManager>();
            IoCManager.Register<ContentLocalizationManager, ContentLocalizationManager>();
            // Goob: to port EE Interaction Verbs. I hate this.
            IoCManager.Register<EntityWhitelistSystem>();
            IoCManager.Register<TagSystem>();
        }
    }
}
