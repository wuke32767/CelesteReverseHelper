--local communalHelper = require("mods").requireFromPlugin("libraries.communal_helper")
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")


local ForceyJellyfish = {}


ForceyJellyfish.name = "ReverseHelper/ForceyJellyfish"
ForceyJellyfish.minimumSize = {16, 16}
ForceyJellyfish.maximumSize = {16, 16}


ForceyJellyfish.placements = {}
    ForceyJellyfish.placements = {
        name = "Forcey Jellyfish",
        data = {
            bubble=false,
            tutorial=false,
            force=120

        }
    }

local texture = "objects/glider/idle0"
-- function customCassetteBlock.sprite(room, entity)
--     return communalHelper.getCustomCassetteBlockSprites(room, entity)
-- end

function ForceyJellyfish.sprite(room, entity)
    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(texture, entity)

        table.insert(lineSprites, 1, jellySprite)

        return lineSprites
    else
        return drawableSprite.fromTexture(texture, entity)
    end
end

function ForceyJellyfish.rectangle(room, entity)
    return utils.rectangle(entity.x - 14, entity.y - 15, 30, 19)
end


return ForceyJellyfish







