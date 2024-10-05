local zipline = {}

--local versionCheck = require("mods").requireFromPlugin("libraries.MoreLoennPlugins.versionCheck")
--if not versionCheck.validModVersion("IsaGrabBag", "1.6.14")
--  then return {}
--end
zipline.associatedMods = { "ReverseHelper","IsaGrabBag" }

zipline.name = "ReverseHelper/ZiplineZipmover"
zipline.depth = 0
zipline.texture = "isafriend/objects/zipline/handle"
zipline.nodeTexture = "isafriend/objects/zipline/handle_end"
zipline.nodeLimits = {1, -1}
zipline.nodeVisibility = "always"
zipline.nodeLineRenderType = "line"
local conserveoption = {
    "None",
    "SameDirection",
    "All",
    "AllWithDirection",
}

zipline.fieldInformation={
    conserveSpeedMode = {
        options = conserveoption
    },

    waitings= {
        fieldType = "list",
    },
    startings= {
        fieldType = "list",
    },
    returnWaitings= {
        fieldType = "list",
    },
    returnStartings= {
        fieldType = "list",
    },
    _padding = {
        fieldType = "ReverseHelper.Padding",
    },
}
zipline.fieldOrder={"x","y","sprite","maxSpeed","time","fixEndSpeed","strict","useStamina","_padding","_padding",
"conserveSpeedMode","conserveRate","conserveLimit","conserveMoving","conserveReturning","ignoreNegative",}
zipline.placements = {
    name = "Zipline Zipmover",
    data = {
        useStamina = true,
        time="",
        maxSpeed="",
        strict=false,
        sprite="",
        conserveSpeedMode="None",
        conserveRate=1,
        --conserveLimit=-1,
        fixEndSpeed = true,
        fixbugsv1=true,
        conserveMoving=false,
        conserveReturning=false,
        ignoreNegative = false,
        
        stoppings = "0.4",
        startings="0.1",
        returnStoppings = "0.4",
        returnStartings = "0.1",
        permanent=false,
        waiting=false,
        ticking = false,
        _padding=false,
    }
}

return zipline
