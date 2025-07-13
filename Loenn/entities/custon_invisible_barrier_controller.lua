local utils = require "utils"

local cbarea = {
}
cbarea.name = "ReverseHelper/BarrierInteropHelper"
cbarea.depth = 8500
cbarea.fieldInformation = {
    zzdoc = {
        fieldType = "ReverseHelper.DOCUMENT",
        important = true,
    },
}
cbarea.placements = {

    name = "Custom Invisible Barrier Fix Controller (Map Wide)",

    data = {
        zzdoc = "",
        actorsolid=true,
        jump=true,
        mod=true,
        jelly=false,
        wind=false,
    }
    
}

cbarea.texture="objects/ReverseHelper/Loenn/InvisibleBarrierController"
return cbarea
