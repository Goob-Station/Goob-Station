# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Kara D <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Composant GasTank

# Texte d'examen indiquant la pression dans la bonbonne.
comp-gas-tank-examine = Pression : [color=orange]{PRESSURE($pressure)}[/color].

# Texte d'examen quand les internals sont actifs.
comp-gas-tank-connected = Elle est connectée à un composant externe.

# Texte d'examen selon l'état de la vanne.
comp-gas-tank-examine-open-valve = La vanne de libération de gaz est [color=red]ouverte[/color].
comp-gas-tank-examine-closed-valve = La vanne de libération de gaz est [color=green]fermée[/color].

## ControlVerb
control-verb-open-control-panel-text = Ouvrir le panneau de contrôle

## UI
gas-tank-window-internals-toggle-button = Basculer
gas-tank-window-output-pressure-label = Pression de sortie
gas-tank-window-tank-pressure-text = Pression : {$tankPressure} kPA
gas-tank-window-internal-text = Internals : {$status}
gas-tank-window-internal-connected = [color=green]Connecté[/color]
gas-tank-window-internal-disconnected = [color=red]Déconnecté[/color]

## Vanne
comp-gas-tank-open-valve = Ouvrir la vanne
comp-gas-tank-close-valve = Fermer la vanne
