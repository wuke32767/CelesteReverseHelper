if not require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    return
end
local utils = require("utils")

local purpleBooster = {}
local option={"right","rightdown","down","leftdown","left","leftup","up","rightup",}


purpleBooster.name = "ReverseHelper/DirectionalBooster"
purpleBooster.depth = -8500

purpleBooster.placements = {
    name = "Directional Booster",
    data = {
        right=true,
        rightdown=true,
        down=true,
        leftdown=true,
        left=true,
        leftup=true,
        up=true,
        rightup = true,
        default = "right",
        red = false,
    }
}
purpleBooster.fieldInformation = {
    default = {
        options = option,
        editable=false,
    }
}

function purpleBooster.texture(room, entity)
    if entity.red then
        return "objects/booster/boosterRed00"
    end
    return "objects/booster/booster00"
end


return purpleBooster
