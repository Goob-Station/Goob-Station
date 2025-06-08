## SuspicionGui.xaml.cs

# Shown when clicking your Role Button in Suspicion
suspicion-ally-count-display = {$allyCount ->
    *[zero] У вас немає союзників
    [one] Ваш союзник {$allyNames}
    [other] Ваші союзники {$allyNames}
}