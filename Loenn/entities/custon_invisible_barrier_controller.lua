local utils = require "utils"

local cbarea = {
}
cbarea.name = "ReverseHelper/BarrierInteropHelper"
cbarea.depth = 8500
cbarea.fieldInformation = {
}
cbarea.placements = {

    name = "Custom Invisible Barrier Fix Controller (Map Wide)",

    data = {
        actorsolid=true,
        jump=true,
        mod=true,
        jelly=false,
    }
    
}

cbarea.texture="objects/ReverseHelper/Loenn/InvisibleBarrierController"
return cbarea
