local OnlyStrawberryCollectTrigger = {}

OnlyStrawberryCollectTrigger.name = "ReverseHelper/OnlyStrawberryCollectTrigger"

OnlyStrawberryCollectTrigger.placements = {
    {
        name = "Specific Berry Collect Trigger",
        data = {
            type="Celeste.Strawberry",
            delayBetweenBerries=false
        }
    }
}

return OnlyStrawberryCollectTrigger
