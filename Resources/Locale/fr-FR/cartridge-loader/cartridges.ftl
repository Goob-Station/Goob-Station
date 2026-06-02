# SPDX-FileCopyrightText: 2022 Aru Moon <anton17082003@gmail.com>
# SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 MishaUnity <81403616+MishaUnity@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Phill101 <28949487+Phill101@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Phill101 <holypics4@gmail.com>
# SPDX-FileCopyrightText: 2024 ArchRBX <5040911+ArchRBX@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lapatison <100279397+lapatison@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Эдуард <36124833+Ertanic@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

device-pda-slot-component-slot-name-cartridge = Cartouche

default-program-name = Programme
notekeeper-program-name = Bloc-notes
nano-task-program-name = NanoTâche
news-read-program-name = Actualités de la station

crew-manifest-program-name = Manifeste de l'équipage
crew-manifest-cartridge-loading = Chargement...

net-probe-program-name = SondeRéseau
net-probe-scan = {$device} scanné !
net-probe-label-name = Nom
net-probe-label-address = Adresse
net-probe-label-frequency = Fréquence
net-probe-label-network = Réseau

log-probe-program-name = SondeJournal
log-probe-scan = Journaux téléchargés depuis {$device} !
log-probe-label-time = Heure
log-probe-label-accessor = Consulté par
log-probe-label-number = #
log-probe-print-button = Imprimer les journaux
log-probe-printout-device = Appareil scanné : {$name}
log-probe-printout-header = Derniers journaux :
log-probe-printout-entry = #{$number} / {$time} / {$accessor}

astro-nav-program-name = AstroNav

med-tek-program-name = MedTek

# NanoTask cartridge

nano-task-ui-heading-high-priority-tasks = 
    { $amount ->
        [zero] Aucune tâche haute priorité
        [one] 1 tâche haute priorité
       *[other] {$amount} tâches haute priorité
    }
nano-task-ui-heading-medium-priority-tasks = 
    { $amount ->
        [zero] Aucune tâche priorité moyenne
        [one] 1 tâche priorité moyenne
       *[other] {$amount} tâches priorité moyenne
    }
nano-task-ui-heading-low-priority-tasks = 
    { $amount ->
        [zero] Aucune tâche basse priorité
        [one] 1 tâche basse priorité
       *[other] {$amount} tâches basse priorité
    }
nano-task-ui-done = Terminé
nano-task-ui-revert-done = Annuler
nano-task-ui-priority-low = Bas
nano-task-ui-priority-medium = Moyen
nano-task-ui-priority-high = Élevé
nano-task-ui-cancel = Annuler
nano-task-ui-print = Imprimer
nano-task-ui-delete = Supprimer
nano-task-ui-save = Enregistrer
nano-task-ui-new-task = Nouvelle tâche
nano-task-ui-description-label = Description :
nano-task-ui-description-placeholder = Obtenir quelque chose d'important
nano-task-ui-requester-label = Demandeur :
nano-task-ui-requester-placeholder = Jean Nanotrasen
nano-task-ui-item-title = Modifier la tâche
nano-task-printed-description = [bold]Description[/bold] : {$description}
nano-task-printed-requester = [bold]Demandeur[/bold] : {$requester}
nano-task-printed-high-priority = [bold]Priorité[/bold] : [color=red]Élevée[/color]
nano-task-printed-medium-priority = [bold]Priorité[/bold] : Moyenne
nano-task-printed-low-priority = [bold]Priorité[/bold] : Basse

# Wanted list cartridge
wanted-list-program-name = Liste des recherchés
wanted-list-label-no-records = Tout va bien, cowboy
wanted-list-search-placeholder = Rechercher par nom et statut

wanted-list-age-label = [color=darkgray]Âge :[/color] [color=white]{$age}[/color]
wanted-list-job-label = [color=darkgray]Poste :[/color] [color=white]{$job}[/color]
wanted-list-species-label = [color=darkgray]Espèce :[/color] [color=white]{$species}[/color]
wanted-list-gender-label = [color=darkgray]Genre :[/color] [color=white]{$gender}[/color]

wanted-list-reason-label = [color=darkgray]Raison :[/color] [color=white]{$reason}[/color]
wanted-list-unknown-reason-label = raison inconnue

wanted-list-initiator-label = [color=darkgray]Initiateur :[/color] [color=white]{$initiator}[/color]
wanted-list-unknown-initiator-label = initiateur inconnu

wanted-list-status-label = [color=darkgray]statut :[/color] {$status ->
        [suspected] [color=yellow]suspected[/color]
        [wanted] [color=red]wanted[/color]
        [detained] [color=#b18644]detained[/color]
        [paroled] [color=green]paroled[/color]
        [discharged] [color=green]discharged[/color]
        [search] [color=#33cccc]search[/color]
        [perma] [color=#343434]perma[/color]
        [dangerous] [color=red]dangerous[/color]
        [demote] [color=red]demote[/color]
        *[other] none
    }

wanted-list-history-table-time-col = Heure
wanted-list-history-table-reason-col = Crime
wanted-list-history-table-initiator-col = Initiateur
