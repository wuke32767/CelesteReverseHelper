if not require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    return
end
local utils = require("utils")

local purpleBooster = {}

purpleBooster.name = "ReverseHelper/TilesetDepthSetter"
purpleBooster.depth = -15000000

purpleBooster.placements = {
    name = "Tileset Depth Setter",
    data = {
        depth = "",
        width=8,height=8,
        fg=true,
        bg=false,
    }
}

purpleBooster.borderColor = { 1, 1, 0, 1 }
purpleBooster.fillColor = { 0, 0, 0, 0 }

return purpleBooster
