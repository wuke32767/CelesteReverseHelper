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
    local onlyEnable = entity.onlyEnable
    local onlyDisable = entity.onlyDisable

    if onlyDisable then
        return "objects/ReverseHelper/DreamToggleSwitch/switch13"
    elseif onlyEnable then
        return "objects/ReverseHelper/DreamToggleSwitch/switch15"
    else
        return "objects/ReverseHelper/DreamToggleSwitch/switch01"
    end
end

function bgModeToggle.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local onlyDisable = entity.onlyDisable
    
    if onlyDisable then
        return utils.rectangle(x - 8, y - 14, 16, 23)
    else
        return utils.rectangle(x - 8, y - 6, 16, 20)
    end
    
end

return bgModeToggle