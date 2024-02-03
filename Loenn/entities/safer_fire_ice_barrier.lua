local utils = require "utils"

local cbarea = {
    fillColor = { 1, 1, 1, 0.1 },
    borderColor = { 1, 1, 1, 0.4 },
}
cbarea.name = "ReverseHelper/SaferFireIceBarrier"
cbarea.depth = 8500
cbarea.associatedMods = { "ReverseHelper" }
cbarea.fieldOrder = { "x","y","width","height","hot","cold","none","iceBlock", }

cbarea.placements = {

    name = "Safer Fire Ice Barrier",

    data = {
        width = 8, height = 8,
        iceBlock=false,
        cold=false,
        hot=true,
        none=true,
        topSafer=false,
        topDangerous=false,
        topRefillDash=true,
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
