local drawable = require("structs.drawable_rectangle")
local drawables = require("structs.drawable_sprite")
local RefillWall={}
local RefillWallWrapper={}

RefillWallWrapper.name="ReverseHelper/ForceyHoldables"

RefillWallWrapper.canResize={true,true}



RefillWallWrapper.placements={
    name="Forcey Holdables",
    data={
            width = 8, height = 8,
            type="Celeste.Glider", 
            force=120

        }
    }
function RefillWallWrapper.sprite(room,entity)



    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16


    local sp=drawable.fromRectangle("line",x,y,width,height,"0000cc")

    return {sp}
end
return {RefillWallWrapper}
