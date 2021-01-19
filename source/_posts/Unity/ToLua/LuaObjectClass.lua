--[[
    Lua 的所有类的顶级类,所有 Lua 都从这个类派生
]]

LuaObjectClass = {}
LuaObjectClass.Name = "LuaObjectClass"

-- 顶级类的创建方法
function LuaObjectClass:New(o)
    local o = o or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

function LuaObjectClass:ToString()
    
    local arg = "\n\n----------------------".. self.Name .." 类参数----------------------"
    for k,v in pairs(self) do
        arg = arg.. "\n" .. k .. "  " .. tostring(v)
    end

    local inheritLink = {}
    table.insert(inheritLink,self.Name .. "---->")

    local parentClass = getmetatable(self)
    while parentClass ~= nil do
        table.insert(inheritLink,parentClass.Name .. "---->")

        arg = arg.. "\n\n----------------------".. parentClass.Name .." 类参数----------------------"
        for k,v in pairs(parentClass) do
            arg = arg.. "\n" .. k .. "  " .. tostring(v)
        end
        parentClass = getmetatable(parentClass)
    end
    arg = "----------------------继承链----------------------\n" .. table.concat(inheritLink) .. "nil" .. arg .. "\n\n"
    return arg
end

-------------继承自顶级类的写法

UIController = LuaObjectClass:New({Name = "UIController"}) -- 管理一个UI模块
print(UIController:ToString())

function UIController:New(o)
    local o = o or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

UIController1 = UIController:New()
UIController1.Name = "UIController1"
print(UIController1:ToString())

-------------继承自顶级类的写法

UIView = LuaObjectClass:New({Name = "UIView"}) -- 小型控件

function UIView:New(o)
    local o = o or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

UIView1 = UIView:New()
UIView1.Name = "UIView1"
print(UIView1:ToString())


-------------继承自顶级类的写法

UIModel = LuaObjectClass:New({Name = "UIModel"}) -- 一个模块的所有数据

function UIModel:New(o)
    local o = o or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

UIModel1 = UIModel:New()
UIModel1.Name = "UIModel1"
print(UIModel1:ToString())