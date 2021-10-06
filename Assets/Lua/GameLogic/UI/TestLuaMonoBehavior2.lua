local TestLuaMonoBehavior = require("GameLogic.UI.TestLuaMonoBehavior")
local TestLuaMonoBehavior2 = Class(TestLuaMonoBehavior)

function TestLuaMonoBehavior2:Awake()
    local cnt = 1
    Log.Info("TestLuaMonoBehavior2:Awake")
    LuaTimer.AddTimer(function()
        Log.Info("Lua Timer " ..cnt)
        cnt = cnt + 1
    end, 1, false, 3, 5)

    local co = nil
    co = Coroutine.Start(function()
        Log.Info("Coroutine Start 1")
        self.image.color = Color.yellow
        Coroutine.WaitForSeconds(3)
        self.image.color = Color.grey
        Log.Info("Coroutine Start 2")

        local i = 0
        Coroutine.WaitUntil(function()
            i = i + 1
            return i >= 10
        end)

        local moveDir = 300
        local moveTime = 1
        local accumulate = 0
        local pos = self.image.transform.anchoredPosition;
        local startX = pos.x
        Coroutine.WaitUntil(function()
            accumulate = accumulate + Time.deltaTime
            pos.x = startX + accumulate / moveTime * moveDir
            self.image.transform.anchoredPosition = pos
            return accumulate >= moveTime
        end)

        Log.Info("Coroutine Start i = %d", i)

        Log.Info("YieldStart 000")
        Coroutine.YieldStart(function(a, b, c)
            Log.Info("YieldStart 111")
            Coroutine.WaitForSeconds(5)
            self.image.color = Color.magenta
            Log.Info("a = %s b = %s c = %s", a, b, c)
        end, "aa", "bb", "cc")

        Log.Info("WWW Yield Start")
        Coroutine.YieldStart(function(url)
            Log.Info("Request Url " ..url)
            local request = CS.UnityEngine.Networking.UnityWebRequest.Get(url)
            request:SendWebRequest()
            Coroutine.WWW(request)
            Log.Info("Http Result:\n%s", request.downloadHandler.text)
        end, "https://www.baidu.com")

        Coroutine.YieldIPairs({1, 2, 3}, function(k, v)
            Log.Info("YieldIPairs k = %s v = %s", k, v)
        end)

        Coroutine.YieldPairs({a = 1, b = 2, c = "aaa"}, function(k, v)
            Log.Info("YieldPairs k = %s v = %s", k, v)
        end)
    end)

    UIPanelManager.OpenUI(UIPanelDef.Main)
    -- Coroutine.Stop(co)
end
-- function TestLuaMonoBehavior2:FixedUpdate()
--     Log.Trace("TestLuaMonoBehavior2-FixedUpdate")
-- end

-- local speed = 10
-- function TestLuaMonoBehavior2:Update()
--     self.image.color = Random.ColorHSV()
--     Log.Trace("TestLuaMonoBehavior2-Update")
-- 	-- local r = CS.UnityEngine.Vector3.up * CS.UnityEngine.Time.deltaTime * speed
-- 	-- self.transform:Rotate(r)
-- end

-- function TestLuaMonoBehavior2:LateUpdate()
--     Log.Trace("TestLuaMonoBehavior2-LateUpdate")
-- end

Log.Trace("bbbbbbbbbbbbbbbbbbbbbbb")

return TestLuaMonoBehavior2