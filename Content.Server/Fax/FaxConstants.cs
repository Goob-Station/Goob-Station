// SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2023 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
// SPDX-FileCopyrightText: 2024 dffdff2423 <dffdff2423@gmail.com>
// SPDX-FileCopyrightText: 2024 exincore <me@exin.xyz>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Fax;

public static class FaxConstants
{
    // Commands

    /**
     * Used to get other faxes connected to current network
     */
    public const string FaxPingCommand = "fax_ping";

    /**
     * Used as response to ping command
     */
    public const string FaxPongCommand = "fax_pong";

    /**
     * Used when fax sending data to destination fax
     */
    public const string FaxPrintCommand = "fax_print";

    // Data

    public const string FaxNameData = "fax_data_name";
    public const string FaxPaperNameData = "fax_data_title";
    public const string FaxPaperLabelData = "fax_data_label";
    public const string FaxPaperPrototypeData = "fax_data_prototype";
    public const string FaxPaperContentData = "fax_data_content";
    public const string FaxPaperStampStateData = "fax_data_stamp_state";
    public const string FaxPaperStampedByData = "fax_data_stamped_by";
    public const string FaxSyndicateData = "fax_data_i_am_syndicate";
    public const string FaxPaperLockedData = "fax_data_locked";
}