local zipline = {}

--local versionCheck = require("mods").requireFromPlugin("libraries.MoreLoennPlugins.versionCheck")
--if not versionCheck.validModVersion("IsaGrabBag", "1.6.14")
--  then return {}
--end
zipline.associatedMods = { "ReverseHelper", "IsaGrabBag" }

zipline.name = "ReverseHelper/ZiplineZipmover"
zipline.depth = 0
zipline.texture = "isafriend/objects/zipline/handle"
zipline.nodeTexture = "isafriend/objects/zipline/handle_end"
zipline.nodeLimits = { 1, -1 }
zipline.nodeVisibility = "always"
zipline.nodeLineRenderType = "line"
local conserveoption = {
    "None",
    "SameDirection",
    "All",
    "AllWithDirection",
}

zipline.fieldInformation = {
    conserveSpeedMode = {
        options = conserveoption
    },

    stoppings = {
        fieldType = "list",
    },
    startings = {
        fieldType = "list",
    },
    returnStoppings = {
        fieldType = "list",
    },
    returnStartings = {
        fieldType = "list",
    },
    _padding = {
        fieldType = "ReverseHelper.Padding",
    },
    ropeColor = {
        fieldType = "color",
    },
}
zipline.fieldOrder = {
    "x", "y",
    "fixEndSpeed", "fixbugsv1", "sprite",
    "maxSpeed", "time",
    "strict", "useStamina", "_padding", "_padding",
    "conserveSpeedMode", "conserveRate",
    "conserveMoving", "conserveReturning", "ignoreNegative",
    "stoppings", "startings",
    "returnStoppings", "returnStartings",
    "permanent", "waiting", "ticking", --"synced",
    "tickDelay", "ticks",
    --"ropeColor", "syncTag",
    "synced", "noReturn", "_padding", "_padding",
    "ropeColor", "syncTag",
    "touchSfx",
    "impactSfx",
    "returnSfx",
    "resetSfx",
    "tickingSfx",
    "dieSfx",
    "adsfcewasdferw",

}
zipline.placements = {
    name = "Zipline Zipmover",
    data = {
        useStamina = true,
        time = "",
        maxSpeed = "",
        strict = false,
        sprite = "",
        conserveSpeedMode = "None",
        conserveRate = 1,
        --conserveLimit=-1,
        fixEndSpeed = true,
        fixbugsv1 = true,
        conserveMoving = false,
        conserveReturning = false,
        ignoreNegative = false,

        stoppings = "0.4",
        startings = "0.1",
        returnStoppings = "0.4",
        returnStartings = "0.1",
        permanent = false,
        waiting = false,
        ticking = false,
        tickDelay = 1,
        ticks = 5,
        _padding = false,
    }
}
if require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    zipline.placements.data=($({
        synced = false,
        ropeColor = "663931",
        syncTag = "",
        noReturn = false,
        resetSfx = "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/reset",
        returnSfx =  "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/return",
        impactSfx =  "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/impact",
        touchSfx =  "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/touch",
        tickingSfx =  "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/touch",
        dieSfx =  "event:/ReverseHelper/VanillaTweaks/Zip_Unzipped/touch",
    }) .. $(zipline.placements.data))()
end
return zipline
