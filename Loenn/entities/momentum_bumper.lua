if not require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    return
end
local utils = require "utils"

local styles = {
    "Green",
    "Orange"
}

local MomentumBumper = {}

MomentumBumper.name = "ReverseHelper/MomentumBumper"

MomentumBumper.nodeLineRenderType = "line"
MomentumBumper.nodeLimits = {0, 1}


MomentumBumper.fieldInformation = {
    mat = {
        fieldType = "uyvbuHelper.attachGroup",
    },
}
MomentumBumper.placements = {
    name = "Momentum Bumper",
    data = {
        mat="",
    }
}


function MomentumBumper.texture(room, entity)
    return "objects/VortexHelper/vortexCustomBumper/" .. "Green" .. "22"
end

function MomentumBumper.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {}

    local nodeRects = {}
    for i, node in ipairs(nodes) do
        nodeRects[i] = utils.rectangle(node.x - 11, node.y - 11, 22, 22)
    end

    return utils.rectangle(x - 11, y - 11, 22, 22), nodeRects
end

return MomentumBumper
