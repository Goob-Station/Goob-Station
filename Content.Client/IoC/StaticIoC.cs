// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.ResourceManagement;

namespace Content.Client.IoC
{
    public static class StaticIoC
    {
        public static IResourceCache ResC => IoCManager.Resolve<IResourceCache>();
    }
}