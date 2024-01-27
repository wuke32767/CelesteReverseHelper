local utils = require("utils")

local bgModeToggle = {}

bgModeToggle.name = "ReverseHelper/DreamToggle"
bgModeToggle.depth = 2000
bgModeToggle.placements = {
    {
        name = "Dream Toggle",
        data = {
            onlyDisable = false,
            onlyEnable = false,
        }
    }
}

function bgModeToggle.texture(room, entity)
    local onlyOn = entity.onlyOn
    local onlyOff = entity.onlyOff

    if onlyOff then
        return "objects/BGswitch/bgflipswitch/switch13"
    elseif onlyOn then
        return "objects/BGswitch/bgflipswitch/switch15"
    else
        return "objects/BGswitch/bgflipswitch/switch01"
    end
end

function bgModeToggle.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local onlyOff = entity.onlyOff
    
    if onlyOff then
        return utils.rectangle(x - 8, y - 14, 16, 23)
    else
        return utils.rectangle(x - 8, y - 6, 16, 20)
    end
    
end

return bgModeToggle