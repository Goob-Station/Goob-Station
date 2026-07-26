manifest-listing-entry-start = (Used { $spent })
manifest-listing-entry-listing = [font size=30]\[[tex path="{ $sprite }" state="{ $state }" offsetY=-12 tooltip="{ $info }"]{ $amount ->
        [1] { "" }
       *[other] x{ $amount }
    }\][/font]
manifest-listing-entry-info = { $name } - { $spent }
