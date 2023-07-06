--local communalHelper = require("mods").requireFromPlugin("libraries.communal_helper")
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")


local HoldableRefill = {}


HoldableRefill.name = "ReverseHelper/HoldableRefill"
HoldableRefill.minimumSize = {16, 16}
HoldableRefill.maximumSize = {16, 16}


HoldableRefill.placements = {}
    HoldableRefill.placements = {
        name = "Holdable Refill",
        data = {
            twoDash=false,
            oneUse=false,
            refillOnHolding=true,
            floaty=false,
            slowFall=true,
            dashable=false,
            stillRefillOnNoDash=false,
            refilltime=2.5
        }
    }

local getTexture = function(t)
    if not t then 
        return "objects/refill/idle00"
    end
    return "objects/refillTwo/idle00"
end
-- function customCassetteBlock.sprite(room, entity)
--     return communalHelper.getCustomCassetteBlockSprites(room, entity)
-- end

function HoldableRefill.sprite(room, entity)
    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(getTexture(entity.twoDash), entity)

        table.insert(lineSprites, 1, jellySprite)

        return lineSprites
    else
        return drawableSprite.fromTexture(getTexture(entity.twoDash), entity)
    end
end

function HoldableRefill.rectangle(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end


return HoldableRefill







