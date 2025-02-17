local drawable = require("structs.drawable_rectangle")
local drawables = require("structs.drawable_sprite")
local RefillWall={}
local RefillWallWrapper={}
RefillWallWrapper.depth = -100000
RefillWallWrapper.name="ReverseHelper/ReversedDreamBlockContainer"

RefillWallWrapper.canResize={true,true}

RefillWallWrapper.fieldInformation = {
    alwaysEnable = {
        fieldType = "ReverseHelper.OptionalBool",
    },
    alwaysDisable = {
        fieldType = "ReverseHelper.OptionalBool",
    }, 
    highPriority = {
        fieldType = "ReverseHelper.OptionalBool",
    },
    reverse = {
        fieldType = "ReverseHelper.OptionalBool",
    },
    touchMode = {
        fieldType = "ReverseHelper.OptionalBool",
    },
    useEntryAngle = {
        fieldType = "ReverseHelper.OptionalBool",
    },
    ghostMode = {
        fieldType = "ReverseHelper.OptionalBool",
    },
    ghostDisableCollidable = {
        fieldType = "ReverseHelper.OptionalBool",
    },
    disableIsSolid = {
        fieldType = "ReverseHelper.OptionalBool",
    },
}


RefillWallWrapper.placements={
    name = "ReversedDreamBlockContainer",
    data={
            width = 8, height = 8,
            alwaysEnable="",
            alwaysDisable="",
            highPriority="",
            reverse=true,
            useEntryAngle = "",
            --disableIsSolid = "",
        }
}

if require("mods").requireFromPlugin("libraries.private", "ReverseHelperPrivate") then
    RefillWallWrapper.placements.data.touchMode = ""
    RefillWallWrapper.placements.data.ghostMode = ""
    RefillWallWrapper.placements.data.ghostDisableCollidable = ""
end

function RefillWallWrapper.sprite(room,entity)



    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16


    local sp=drawable.fromRectangle("line",x,y,width,height,"00cccc")

    return {sp}
end
return {RefillWallWrapper}
