// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.PatreonParser;

public readonly record struct Patron(string FullName, string TierName, DateTime Start);
