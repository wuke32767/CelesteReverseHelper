if not require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    return
end
local utils = require "utils"
local fakeTilesHelper = require("helpers.fake_tiles")
local drawable = require("structs.drawable_rectangle")

local cbarea = {
}
cbarea.nodeVisibility = "always"
cbarea.nodeLineRenderType = "fan"
cbarea.nodeLimits = { -1, -1 }
cbarea.name = "ReverseHelper/EfficientSpinner"
cbarea.depth = -10000000
cbarea.associatedMods = { "ReverseHelper" }
cbarea.placements = {

    name = "Efficient Spinner",

    data = {
        ReadMe = "",
        texture = "objects/ReverseHelper/SquareSpinner/fg_neon",
    },

}

--local fakeTilesSpriteFunction = fakeTilesHelper.getEntitySpriteFunction("tiles", false)

function cbarea.texture(room, entity)
    return entity.texture
end

return cbarea
