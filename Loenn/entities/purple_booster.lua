local utils = require "utils"

local purpleBooster = {}

purpleBooster.name = "ReverseHelper/AnotherPurpleBooster"
purpleBooster.depth = -8500
purpleBooster.associatedMods = { "ReverseHelper","VortexHelper", }

purpleBooster.placements = {
    
        name = "Another Purple Booster",
        data = {
            redirect=true,
            addDashAttack=false,
            --NewAction=false,
            conserveSpeed= false,
            conserveSpeedV=true,
            conserveMovingSpeedCutOff=1000,
            fixDashDir=false,
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
