require("LuaPanda").start("127.0.0.1",8818)

require("Framework.Init")

GameApp = {}

function GameApp.Start()
    Log.Trace("Game App Start...")
    Log.Trace("sss->")

    for k, v in pairs(_G) do
        Log.Info(string.format("%s %s", k, v))
    end

    Log.Trace(rapidjson.encode({
        ['k'] = 123,
    }))

    Log.Trace(rapidjson.encode({}))

    -- local i = 1
    -- TimerManager.AddTimer(function()
    --     Log.Trace(string.format("frameCount %d times %d", Time.frameCount, i))
    --     i = i + 1
    -- end, 0)

    Log.Trace(string.format("Target Frame Rate : " ..Application.targetFrameRate))

    Log.Trace("%s %d go go go", "loggggg", 123)
end

xpcall(GameApp.Start, function(errorMessage)
    Log.Error(string.format("%s\n%s", errorMessage, debug.traceback()))
end)