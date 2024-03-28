local utils = require "utils"
local fakeTilesHelper = require("helpers.fake_tiles")
local drawable = require("structs.drawable_rectangle")

local cbarea = {
}
cbarea.name = "ReverseHelper/ImmovableBlendFancyTileDreamBlock"
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

}

cbarea.placements = {

    name = "Immovable Blend Fancy Tile Dream Block",

    data = {
        width = 8,
        height = 8,
        tileData = "",
        lineColor="FFFFFFFF",
        fillColor="00000000",
        lineColorDeactivated="FFFFFFFF",
        fillColorDeactivated="0000003f",
        below=false,
        oneUse=false,
        highPriority=false,
    }
    
}


function cbarea.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
   
    entity.x=math.floor(x/8)*8
    entity.y=math.floor(y/8)*8
    entity.width=math.floor(width/8)*8
    entity.height=math.floor(height/8)*8
    x=entity.x
    y=entity.y
    width=entity.width
    height=entity.height
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
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    entity.x=math.floor(x/8)*8
    entity.y=math.floor(y/8)*8
    entity.width=math.floor(width/8)*8
    entity.height=math.floor(height/8)*8
    x=entity.x
    y=entity.y
    width=entity.width
    height=entity.height

    local main = utils.rectangle(x, y, width, height)


    return main,{}
end
return cbarea
