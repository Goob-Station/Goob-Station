// SPDX-FileCopyrightText: 2020 Tyler Young <tyler.young@impromptu.ninja>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Content.IntegrationTests;
using Content.Server.Maps;
#if DEBUG
using BenchmarkDotNet.Configs;
#else
using Robust.Benchmarks.Configs;
#endif
using Robust.Shared.Prototypes;

namespace Content.Benchmarks
{
    internal static class Program
    {

        public static void Main(string[] args)
        {
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nWARNING: YOU ARE RUNNING A DEBUG BUILD, USE A RELEASE BUILD FOR AN ACCURATE BENCHMARK");
            Console.WriteLine("THE DEBUG BUILD IS ONLY GOOD FOR FIXING A CRASHING BENCHMARK\n");
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
#else
            var config = Environment.GetEnvironmentVariable("ROBUST_BENCHMARKS_ENABLE_SQL") != null ? DefaultSQLConfig.Instance : null;
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
#endif
        }
    }
}