// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._NF.Hands.UI
{
    public sealed class HandPlaceholderStatus : Control
    {
        public HandPlaceholderStatus()
        {
            RobustXamlLoader.Load(this);
        }
    }
}