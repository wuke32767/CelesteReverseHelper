local utils = require "utils"

local cbarea = {
    fieldInformation = {
        version = {
            options = { 1, 2, 3 },
            fieldType="integer",
            editable = false,
        }
    }
}


cbarea.name = "ReverseHelper/DreamDashToAndFromFlag"
cbarea.depth = 8500
cbarea.placements = {

    name = "Dream Dash To And From Flag (Map Wide)",

    data = {
        flag="PlayerHasDreamDash",
        version=1,
    }
    
}

cbarea.texture="objects/ReverseHelper/Loenn/DreamDashFlager"
return cbarea
