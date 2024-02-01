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
zipline.nodeLimits = {1, 1}
zipline.nodeVisibility = "always"
zipline.nodeLineRenderType = "line"


zipline.placements = {
    name = "Zipline Zipmover",
    data = {
        useStamina = true,
        time="",
        maxSpeed="",
        strict=false,
        sprite="",
    }
}

return zipline
