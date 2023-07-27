local utils = require "utils"

local purpleBooster = {}

purpleBooster.name = "ReverseHelper/AnotherPurpleBooster"
purpleBooster.depth = -8500
purpleBooster.associatedMods = { "VortexHelper","ReverseHelper" }

purpleBooster.placements = {
    
        name = "Another Purple Booster",
        data = {
            --redirect=true,--not finished
        }
    
}

function purpleBooster.texture(room, entity)
    return "objects/VortexHelper/slingBooster/slingBooster00"
end

function purpleBooster.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 9, y - 9, 18, 18)
end

return purpleBooster
