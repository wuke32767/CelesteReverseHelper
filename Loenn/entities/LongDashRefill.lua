--local communalHelper = require("mods").requireFromPlugin("libraries.communal_helper")
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")


local HoldableRefill = {}
HoldableRefill.associatedMods = { "ExtendedVariantMode","ReverseHelper" }


HoldableRefill.name = "ReverseHelper/LongDashRefill"
HoldableRefill.minimumSize = {16, 16}
HoldableRefill.maximumSize = {16, 16}


HoldableRefill.placements = {}
HoldableRefill.placements = {
    name = "Long Dash Refill",
    data = {
        oneUse=false,
        dashTime=0.01,
        image="objects/ReverseHelper/LongDashRefill/"
    },
}

local getTexture = function(entity)
    return entity.image.."idle00"
end
-- function customCassetteBlock.sprite(room, entity)
--     return communalHelper.getCustomCassetteBlockSprites(room, entity)
-- end

function HoldableRefill.sprite(room, entity)

    return drawableSprite.fromTexture(getTexture(entity), entity)
end

function HoldableRefill.rectangle(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end


return HoldableRefill







