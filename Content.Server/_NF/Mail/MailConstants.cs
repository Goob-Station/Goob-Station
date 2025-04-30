// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Mail
{
    /// <summary>
    /// A set of localized strings related to mail entities
    /// </summary>
    public struct MailEntityStrings
    {
        public string NameAddressed;
        public string DescClose;
        public string DescFar;
    }

    /// <summary>
    /// Constants related to mail.
    /// </summary>
    public sealed class MailConstants : EntitySystem
    {
        /// <summary>
        /// Locale strings related to small parcels.
        /// <summary>
        public static readonly MailEntityStrings Mail = new()
        {
            NameAddressed = "mail-item-name-addressed",
            DescClose = "mail-desc-close",
            DescFar = "mail-desc-far"
        };

        /// <summary>
        /// Locale strings related to large packages.
        /// <summary>
        public static readonly MailEntityStrings MailLarge = new()
        {
            NameAddressed = "mail-large-item-name-addressed",
            DescClose = "mail-large-desc-close",
            DescFar = "mail-large-desc-far"
        };
    }
}