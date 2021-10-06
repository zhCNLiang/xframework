LuaBehaviour = Class()

function LuaBehaviour:Ctor(behaviour, variables)
    local mt = getmetatable(self)
    local super = mt.__index
    setmetatable(self, {
        __index = function(_, k)
           return rawget(variables, k) or super[k] or behaviour[k]
        end
    })
end