# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

logic-gate-examine = Il s'agit actuellement d'une porte {INDEFINITE($gate)} {$gate}.

logic-gate-cycle = Basculé sur une porte {INDEFINITE($gate)} {$gate}

power-sensor-examine = Il vérifie actuellement la batterie { $output ->
    [true] de sortie
    *[false] d'entrée
} du réseau.
power-sensor-voltage-examine = Il surveille le réseau d'alimentation {$voltage}.

power-sensor-switch = Basculé sur la vérification de la batterie { $output ->
    [true] de sortie
    *[false] d'entrée
} du réseau.
power-sensor-voltage-switch = Réseau basculé sur {$voltage} !
