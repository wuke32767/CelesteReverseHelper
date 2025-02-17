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
    zzdoc = {
        fieldType = "ReverseHelper.DOCUMENT",
        important = true,
    },
}
cbarea.placements = {

    name = "Custom Invisible Barrier",

    data = {
        zzdoc = "",
        width = 8, height = 8,
        type="Player,TheoCrystal",
        reversed=false,
        climb=false,
    }
    
}


return cbarea
