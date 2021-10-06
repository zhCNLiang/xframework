local UIMain = Class(UIPanel)

function UIMain:Awake()
    self.super.Awake(self)

    Log.Trace("UIMain Awake")

    self.title.text = "asdfasd"

    self.btn.onClick:AddListener(function()
        UIPanelManager.OpenUI(UIPanelDef.MessageBox, {
            Title = "充值",
            Content = "充值648大礼包",
            LeftBtn = {
                Title = "确认",
                Callback = function()
                    Log.Info("充值成功")
                end
            },
            RightBtn = {
                Title = "取消",
                Callback = function()
                    Log.Info("充值取消")
                end
            }
        })
    end)
end

function UIMain:OnViewShow()
    self.vListView:RegisterItemUpdate(function(item, index, lifeCycle)
        item.elements.text.text = "item_" ..tostring(index)
        -- local height = self.vListView:GetItemRowHeight(index)
        -- item.transform:SetSizeHeight(height)
    end)
    -- self.vListView:RegisterItemRowHeight(function(item, index)
    --     return 100 + index * 30
    -- end)
    self.vListView:SetNum(30)
    self.vListView:ReloadAll();
    self.vListView:ScrollToIndex(8, 0.5)

    self.hListView:RegisterItemUpdate(function(item, index, lifeCycle)
        item.elements.text.text = "item_" ..tostring(index)
        -- local height = self.hListView:GetItemRowHeight(index)
        -- item.transform:SetSizeWidth(height)
    end)
    -- self.hListView:RegisterItemRowHeight(function(item, index)
    --     return 100 + index * 30
    -- end)
    self.hListView:SetNum(30)
    self.hListView:ReloadAll();
    self.hListView:ScrollToIndex(8, 0.5)
end

return UIMain