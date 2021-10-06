function Class(super)
	if super ~= nil and type(super) ~= "table" then
		assert("super must be table")
	end

	local cls = {}

	if super then
		setmetatable(cls, {__index = super})
		cls.super = super
	end

	cls.New = function(...)
		local inst = {}
		setmetatable(inst, {__index = cls})
		if inst.Ctor then
			inst.Ctor(inst, ...)
		end
		return inst
	end

	return cls
end