local utils = require "utils"

local cbarea = {
    fillColor = { 1, 1, 1, 0.2 },
    borderColor = { 1, 1, 1, 0.2 },
}
cbarea.name = "ReverseHelper/CustomInvisibleBarrier"
cbarea.depth = 8500
cbarea.associatedMods = { "ReverseHelper" }
cbarea.fieldInformation =
{
    jumpThruMode = {
        options = {
            "none",
            "left",
            "right",
            "up",
            "down",
        },
        editable = false,
    },
    zzdoc = {
        fieldType = "ReverseHelper.DOCUMENT",
        important = true,
    },
}
cbarea.placements = {

    name = "Custom Invisible Barrier",

    data = {
        zzdoc = "",
        width = 8,
        height = 8,
        type = "Player,TheoCrystal",
        reversed = false,
        climb = false,
    }

}
if require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    cbarea.placements.data.jumpThruMode = "none"
end

return cbarea
