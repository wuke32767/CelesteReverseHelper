local utils = require "utils"
local fakeTilesHelper = require("helpers.fake_tiles")
local drawable = require("structs.drawable_rectangle")

local cbarea = {
}
cbarea.name = "ReverseHelper/FgTileDreamBlockExtractor"
cbarea.depth = -10000000
cbarea.associatedMods = { "ReverseHelper" }

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

}

cbarea.placements = {

    name = "Fg Tile Dream Block Extractor",

    data = {
        width = 8, height = 8,
        lineColor="FFFFFFFF",
        fillColor="00000000",
        lineColorDeactivated="FFFFFFFF",
        fillColorDeactivated="0000003f",
    }
    
}

--local fakeTilesSpriteFunction = fakeTilesHelper.getEntitySpriteFunction("tiles", false)

function cbarea.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local lineColor, fillColor = entity.lineColor or "FFFFFFFF", entity.fillColor or "00000000"

    entity.x=math.round(x/8)*8
    entity.y=math.round(y/8)*8
    entity.width=math.round(width/8)*8
    entity.height=math.round(height/8)*8
    x=entity.x
    y=entity.y
    width=entity.width
    height=entity.height

    --local sprites = fakeTilesSpriteFunction(room, entity)
    local rect2=drawable.fromRectangle("fill",x,y,width,height,fillColor)
    local rect=drawable.fromRectangle("line",x,y,width,height,lineColor)

    rect.depth = -10000000
    rect2.depth = -10000000
    --table.insert(sprites,rect2)
    --table.insert(rect, rect2)

    return {rect,rect2}
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
--function cbarea.selection(room, entity)
--    local main = utils.rectangle(entity.x, entity.y, entity.width, entity.height)
--    local nodes = {}
--
--    if entity.nodes then
--        for i, node in ipairs(entity.nodes) do
--            nodes[i] = utils.rectangle(node.x, node.y, entity.width, entity.height)
--        end
--    end
--
--    return main, nodes
--end
return cbarea
