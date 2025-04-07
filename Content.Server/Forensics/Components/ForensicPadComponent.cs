// SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Server.Forensics
{
    /// <summary>
    /// Used to take a sample of someone's fingerprints.
    /// </summary>
    [RegisterComponent]
    public sealed partial class ForensicPadComponent : Component
    {
        [DataField("scanDelay")]
        public float ScanDelay = 3.0f;

        public bool Used = false;
        public String Sample = string.Empty;
    }
}