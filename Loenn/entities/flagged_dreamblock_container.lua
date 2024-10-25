local drawable = require("structs.drawable_rectangle")
local drawables = require("structs.drawable_sprite")
local RefillWallWrapper={}

RefillWallWrapper.name = "ReverseHelper/FlaggedDreamBlockContainer"

RefillWallWrapper.canResize={true,true}

RefillWallWrapper.fillColor = { 0, 0, 0, 0 }

RefillWallWrapper.borderColor = { 1, 0, 1, 0.4 }



RefillWallWrapper.placements={
    name = "Flagged Dream Block Container",
    data={
            width = 8, height = 8,
            flag="dreamblockgroup2"
        }
    }
return {RefillWallWrapper}
