### Interaction Messages

# Shown when repairing something
comp-repairable-repair = Ви ремонтуєте {PROPER($target) ->
  [true] {""}
  *[false] the{" "}
}{$target} за допомогою {PROPER($tool) ->
  [true] {""}
  *[false] the{" "}
}{$tool}
