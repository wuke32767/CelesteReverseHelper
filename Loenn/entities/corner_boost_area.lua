local utils = require "utils"
local drawable = require("structs.drawable_rectangle")

local cbarea = {
    fillColor = { 1, 1, 1, 0.1 },
    borderColor = { 1, 1, 1, 0.4 },
}
cbarea.name = "ReverseHelper/CornerBoostArea"
cbarea.depth = 8500
cbarea.associatedMods = { "ReverseHelper" }
cbarea.fieldInformation = {
    totalTimes = {
        fieldType = "integer",
    },
    timesBeforeExit = {
        fieldType = "integer",
    },
    fillColor = {
        fieldType = "color",
    },
    color = {
        fieldType = "color",
    },
    lineColor = {
        fieldType = "color",
    }
}

cbarea.placements = {

    name = "Corner Boost Area",

    data = {
        width = 8, height = 8,
        depth=8500,
        timesBeforeExit=1,
        totalTimes=-1,
        cooldown=0,
        cooldownOnExit=false,
        onlyRCB=false,
        cornerHyper=false,
        fillColor="ffffffff",
        lineColor="ffffffff",
        legacyTransparent=true,
    }
    
}

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

return cbarea
