ent-SpawnPointGhostBlob = Gloob spawner
    .suffix = DEBUG, Ghost Role Spawner
    .desc = { ent-MarkerBase.desc }
ent-MobBlobPod = Gloob Drop
    .desc = An ordinary Gloob fighter.
ent-MobBlobBlobbernaut = Gloobbernaut
    .desc = An elite Gloob fighter.
ent-BaseBlob = basic Gloob.
    .desc = { "" }
ent-NormalBlobTile = Regular Tile Gloob
    .desc = An ordinary part of the Gloob required for the construction of more advanced tiles.
ent-CoreBlobTile = Gloob Core
    .desc = The most important organ of the Gloob. By destroying the core, the infection will cease.
ent-FactoryBlobTile = Gloob Factory
    .desc = Spawns Gloob Drops and Gloobbernauts over time.
ent-ResourceBlobTile = Resource Gloob
    .desc = Produces resources for the Gloob.
ent-NodeBlobTile = Gloob Node
    .desc = A mini version of the core that allows you to place special Gloob tiles around itself.
ent-StrongBlobTile = Strong Gloob Tile
    .desc = A reinforced version of the regular tile. It does not allow air to pass through and protects against brute damage.
ent-ReflectiveBlobTile = Gloob Reflective Tiles
    .desc = It reflects lasers, but does not protect against brute damage as well.
    .desc = { "" }
objective-issuer-blob = Gloob


ghost-role-information-blobbernaut-name = Gloobbernaut
ghost-role-information-blobbernaut-description = You are a Gloobbernaut. You must defend the Gloob core.

ghost-role-information-blob-name = Gloob
ghost-role-information-blob-description = You are the Gloob Infection. Consume the station.

roles-antag-blob-name = Gloob
roles-antag-blob-objective = Reach critical mass.

guide-entry-blob = Gloob

# Popups
blob-target-normal-blob-invalid = Wrong Gloob type, select a normal Gloob.
blob-target-factory-blob-invalid = Wrong Gloob type, select a factory Gloob.
blob-target-node-blob-invalid = Wrong Gloob type, select a node Gloob.
blob-target-close-to-resource = Too close to another resource Gloob.
blob-target-nearby-not-node = No node or resource Gloob nearby.
blob-target-close-to-node = Too close to another node.
blob-target-already-produce-blobbernaut = This factory has already produced a Gloobbernaut.
blob-cant-split = You can not split the Gloob core.
blob-not-have-nodes = You have no nodes.
blob-not-enough-resources = Not enough resources.
blob-help = Only God can help you.
blob-swap-chem = In development.
blob-mob-attack-blob = You can not attack a Gloob.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = You are dying while not on Gloob tiles.
carrier-blob-alert = You have { $second } seconds left before transformation.

blob-mob-zombify-second-start = { $pod } starts turning you into a zombie.
blob-mob-zombify-third-start = { $pod } starts turning { $target } into a zombie.

blob-mob-zombify-second-end = { $pod } turns you into a zombie.
blob-mob-zombify-third-end = { $pod } turns { $target } into a zombie.

blobberaut-factory-destroy = factory destroy
blob-target-already-connected = already connected


# UI
blob-chem-swap-ui-window-name = Swap chemicals
blob-chem-reactivespines-info = Reactive Spines
                                Deals 25 brute damage.
blob-chem-blazingoil-info = Blazing Oil
                            Deals 15 burn damage and lights targets on fire.
                            Makes you vulnerable to water.
blob-chem-regenerativemateria-info = Regenerative Materia
                                    Deals 6 brute damage and 15 toxin damage.
                                    The Gloob core regenerates health 10 times faster than normal and generates 1 extra resource.
blob-chem-explosivelattice-info = Explosive Lattice
                                    Deals 5 burn damage and explodes the target, dealing 10 brute damage.
                                    Spores explode on death.
                                    You become immune to explosions.
                                    You take 50% more damage from burns and electrical shock.
blob-chem-electromagneticweb-info = Electromagnetic Web
                                    Deals 20 burn damage, 20% chance to cause an EMP pulse when attacking.
                                    Gloob tiles cause an EMP pulse when destroyed.
                                    You take 25% more brute and heat damage.

blob-alert-out-off-station = The Gloob was removed because it was found outside the station!

# Announcment
blob-alert-recall-shuttle = The emergency shuttle can not be sent while there is a level 5 biohazard present on the station.
blob-alert-detect = Confirmed outbreak of level 5 biohazard aboard the station. All personnel must contain the outbreak.
blob-alert-critical = Biohazard level critical, nuclear authentication codes have been sent to the station. Central Command orders any remaining personnel to activate the self-destruction mechanism.
blob-alert-critical-NoNukeCode = Biohazard level critical. Central Command orders any remaining personnel to seek shelter, and await resque.

# Actions
blob-create-factory-action-name = Place Factory Gloob (80)
blob-create-factory-action-desc = Turns selected normal Gloob into a factory Gloob, which will produce up to 3 spores and a Gloobbernaut if placed next to a core or a node.
blob-create-resource-action-name = Place Resource Gloob (60)
blob-create-resource-action-desc = Turns selected normal Gloob into a resource Gloob which will generates resources if placed next to a core or a node.
blob-create-node-action-name = Place Node Gloob (50)
blob-create-node-action-desc = Turns selected normal Gloob into a node Gloob.
                                A node Gloob will activate effects of factory and resource Gloobs, heal other Gloobs and slowly expand, destroying walls and creating normal Gloobs.
blob-produce-blobbernaut-action-name = Produce a Gloobbernaut (60)
blob-produce-blobbernaut-action-desc = Creates a Gloobbernaut on the selected factory. Each factory can only do this once. The Gloobbernaut will take damage outside of Gloob tiles and heal when close to nodes.
blob-split-core-action-name = Split Core (400)
blob-split-core-action-desc = You can only do this once. Turns selected node into an independent core that will act on its own.
blob-swap-core-action-name = Relocate Core (200)
blob-swap-core-action-desc = Swaps the location of your core and the selected node.
blob-teleport-to-core-action-name = Jump to Core (0)
blob-teleport-to-core-action-desc = Teleports you to your Gloob Core.
blob-teleport-to-node-action-name = Jump to Node (0)
blob-teleport-to-node-action-desc = Teleports you to a random Gloob node.
blob-help-action-name = Help
blob-help-action-desc = Get basic information about playing as Gloob.
blob-swap-chem-action-name = Swap chemicals (70)
blob-swap-chem-action-desc = Lets you swap your current chemical.
blob-carrier-transform-to-blob-action-name = Transform into a Gloob
blob-carrier-transform-to-blob-action-desc = Instantly destoys your body and creates a Gloob core. Make sure to stand on a floor tile, otherwise you will simply disappear.
blob-downgrade-action-name = downgrade Gloob(0)
blob-downgrade-action-desc = Turns the selected tile back into a normal Gloob to install other types of cages.

# Ghost role
blob-carrier-role-name = Gloob carrier
blob-carrier-role-desc =  A blob-infected creature.
blob-carrier-role-rules = You are an antagonist. You have 4 minutes before you transform into a Gloob.
                        Use this time to find a safe spot on the station. Keep in mind that you will be very weak right after the transformation.
blob-carrier-role-greeting = You are a carrier of Gloob. Find a secluded place at the station and transform into a Gloob. Turn the station into a mass and its inhabitants into your servants. We are all Gloobs.

# Verbs
blob-pod-verb-zombify = Zombify
blob-verb-upgrade-to-strong = Upgrade to Strong Gloob
blob-verb-upgrade-to-reflective = Upgrade to Reflective Gloob
blob-verb-remove-blob-tile = Remove Gloob

# Alerts
blob-resource-alert-name = Core Resources
blob-resource-alert-desc = Your resources produced by the core and resource Gloobs. Use them to expand and create special Gloobs.
blob-health-alert-name = Core Health
blob-health-alert-desc = Your core's health. You will die if it reaches zero.

# Greeting
blob-role-greeting =
    You are Gloob - a parasitic space creature capable of destroying entire stations.
        Your goal is to survive and grow as large as possible.
    	You are almost invulnerable to physical damage, but heat can still hurt you.
        Use Alt+LMB to upgrade normal Gloob tiles to strong Gloob and strong Gloob to reflective Gloob.
    	Make sure to place resource Gloobs to generate resources.
        Keep in mind that resource Gloobs and factories will only work when next to node Gloobs or cores.
blob-zombie-greeting = You were infected and raised by a Gloob spore. Now you must help the Gloob take over the station.

# End round
blob-round-end-result =
    { $blobCount ->
        [one] There was one Gloob infection.
        *[other] There were {$blobCount} Gloobs.
    }

blob-user-was-a-blob = [color=gray]{$user}[/color] was a Gloob.
blob-user-was-a-blob-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a Gloob.
blob-was-a-blob-named = [color=White]{$name}[/color] was a Gloob.

preset-blob-objective-issuer-blob = [color=#33cc00]Gloob[/color]

blob-user-was-a-blob-with-objectives = [color=gray]{$user}[/color] was a Gloob who had the following objectives:
blob-user-was-a-blob-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a Gloob who had the following objectives:
blob-was-a-blob-with-objectives-named = [color=White]{$name}[/color] was a Gloob who had the following objectives:

# Objectivies
objective-condition-blob-capture-title = Take over the station
objective-condition-blob-capture-description = Your only goal is to take over the whole station. You need to have at least {$count} Gloob tiles.
objective-condition-success = { $condition } | [color={ $markupColor }]Success![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Failure![/color] ({ $progress }%)
