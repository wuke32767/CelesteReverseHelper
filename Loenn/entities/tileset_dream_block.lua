local utils = require "utils"
local fakeTilesHelper = require("helpers.fake_tiles")
local drawable = require("structs.drawable_rectangle")

local cbarea = {
}
cbarea.name = "ReverseHelper/TileDreamBlock"
cbarea.depth = 8500
cbarea.associatedMods = { "ReverseHelper" }
cbarea.nodeLimits = { 0, 1 }

cbarea.fieldInformation = {

    lineColorDeactivated = {
        fieldType = "color",
    },
    fillColorDeactivated = {
        fieldType = "color",
    },
    lineColor = {
        fieldType = "color",
    },
    fillColor = {
        fieldType = "color",
    },
    tiles =
    {
        options = fakeTilesHelper.getTilesOptions(),
        editable = false

    }

}

cbarea.placements = {

    name = "Tile Dream Block",

    data = {
        width = 8, height = 8,
        tiles = '3',
        blendIn = true,
        lineColor="FFFFFFFF",
        fillColor="00000000",
        lineColorDeactivated="FFFFFFFF",
        fillColorDeactivated="0000003f",
        fastMoving=false,
        oneUse=false,
        below=false,
    }
    
}

local fakeTilesSpriteFunction = fakeTilesHelper.getEntitySpriteFunction("tiles", false)

function cbarea.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local lineColor, blockColor = entity.lineColor or "FFFFFFFF", entity.fillColor or "00000000"

    local sprites = fakeTilesSpriteFunction(room, entity)
    --local rect2=drawable.fromRectangle("fill",x,y,width,height,fillColor)
    local rect=drawable.fromRectangle("line",x,y,width,height,lineColor)

    rect.depth = 0
    --table.insert(sprites,rect2)
    table.insert(sprites, rect)

    return sprites
end
--function cbarea.sprite(room, entity)
--    local x, y = entity.x or 0, entity.y or 0
--    local width, height = entity.width or 16, entity.height or 16
--
--
--    local sp=drawable.fromRectangle("line",x,y,width,height,"ffffff")
--
--    return {sp}
--end

--function cbarea.selection(room, entity)
--    local x, y = entity.x or 0, entity.y or 0
--    return utils.rectangle(x, y, entity.width or 8, entity.height or 8)
--end
function cbarea.selection(room, entity)
    local main = utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    local nodes = {}

    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            nodes[i] = utils.rectangle(node.x, node.y, entity.width, entity.height)
        end
    end

    return main, nodes
end
return cbarea
