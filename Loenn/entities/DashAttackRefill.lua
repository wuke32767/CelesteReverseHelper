if not require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    return
end
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")


local HoldableRefill = {}

HoldableRefill.name = "ReverseHelper/DashAttackRefill"
HoldableRefill.minimumSize = { 16, 16 }
HoldableRefill.maximumSize = { 16, 16 }


HoldableRefill.placements = {}
HoldableRefill.placements = {
    name = "Dash Attack Refill",
    data = {
        oneUse = false,
        dashAttackTime = 0.3,
        respawnTime = 2.5,
        image = "objects/refill/",
    },
}

local getTexture = function(entity)
    return entity.image .. "idle00"
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
