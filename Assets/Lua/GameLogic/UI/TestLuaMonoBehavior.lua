local TestLuaMonoBehavior = Class(LuaBehaviour)

function TestLuaMonoBehavior:Awake()
    Log.Trace("TestLuaMonoBehavior-Awake")
end

function TestLuaMonoBehavior:OnEnable()
    Log.Trace("TestLuaMonoBehavior-OnEnable")
end

function TestLuaMonoBehavior:Start()
    Log.Trace("TestLuaMonoBehavior-Start")
end

-- function TestLuaMonoBehavior:FixedUpdate()
--     Log.Trace("TestLuaMonoBehavior-FixedUpdate")
-- end

-- function TestLuaMonoBehavior:Update()
--     Log.Trace("TestLuaMonoBehavior-Update")
-- end

-- function TestLuaMonoBehavior:LateUpdate()
--     Log.Trace("TestLuaMonoBehavior-LateUpdate")
-- end

function TestLuaMonoBehavior:OnDisable()
    Log.Trace("TestLuaMonoBehavior-OnDisable")
end

function TestLuaMonoBehavior:OnDestroy()
    Log.Trace("TestLuaMonoBehavior-OnDestroy")
end

Log.Trace("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")

return TestLuaMonoBehavior