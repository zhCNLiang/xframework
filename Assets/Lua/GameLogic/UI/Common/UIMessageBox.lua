local UIMessageBox = Class(UIPanel)

local function OnClose()
    UIPanelManager.CloseUI(UIPanelDef.MessageBox)
end

function UIMessageBox:Awake()
    self.super.Awake(self)

    self.confirm.onClick:AddListener(Handler(self, self.OnClickOK))
    self.cancel.onClick:AddListener(Handler(self, self.OnClickCancel))
end

function UIMessageBox:OnDestroy()
    self.super.OnDestroy(self)

    self.confirm.onClick:RemoveAllListeners()
    self.cancel.onClick:RemoveAllListeners()
end

function UIMessageBox:OnDoAnimationShow(onViewShow)
    -- self.viewGraphicRaycaster.enabled = false
    local nStartScale = 0.5
    local nToScale = 1
    local nDuration = 0.05
    local nAccmulate = 0
    self.backGround.localScale = Vector3(0.5, 0.5, 0.5)
    Coroutine.Start(function()
        Coroutine.WaitUntil(function()
            nAccmulate = nAccmulate + Time.deltaTime
            local t = nAccmulate / nDuration
            local scale = nStartScale + (nToScale - nStartScale) * t
            self.backGround.localScale = Vector3(scale, scale, scale)
            return scale >= nToScale
        end)

        onViewShow()
        -- self.viewGraphicRaycaster.enabled = true
    end)
end

function UIPanel:OnDoAnimationHide(onViewHide)
    -- self.viewGraphicRaycaster.enabled = false
    local nStartScale = 1
    local nToScale = 0.5
    local nDuration = 0.05
    local nAccmulate = 0
    self.backGround.localScale = Vector3(0.5, 0.5, 0.5)
    Coroutine.Start(function()
        Coroutine.WaitUntil(function()
            nAccmulate = nAccmulate + Time.deltaTime
            local t = nAccmulate / nDuration
            local scale = nStartScale + (nToScale - nStartScale) * t
            self.backGround.localScale = Vector3(scale, scale, scale)
            return scale <= nToScale
        end)

        onViewHide()
        -- self.viewGraphicRaycaster.enabled = true
    end)
end

function UIMessageBox:OnViewShow()
    self.super.OnViewShow(self)
    Log.Trace("UIMessageBox Show")
end

function UIMessageBox:OnViewHide()
    self.super.OnViewHide(self)
    Log.Trace("UIMessageBox Hide")
end

function UIMessageBox:OnOpen(tStyles)
    local strTile = tStyles.Title or ""
    local strContent = tStyles.Content or ""
    local leftTxt = tStyles.LeftBtn and tStyles.LeftBtn.Title or ""
    local leftCallback = tStyles.LeftBtn and tStyles.LeftBtn.Callback or OnClose
    local rightTxt = tStyles.RightBtn and tStyles.RightBtn.Title or ""
    local rightCallback = tStyles.RightBtn and tStyles.RightBtn.Callback or OnClose

    self.title.text = strTile
    self.content.text = strContent
    self.confirmTxt.text = leftTxt
    self.cancelTxt.text = rightTxt

    self.confirmCallback = leftCallback or OnClose
    self.cancelCallback = rightCallback or OnClose

    self.confirm.gameObject:SetActive(tStyles.LeftBtn ~= nil)
    self.cancel.gameObject:SetActive(tStyles.RightBtn ~= nil)
end

function UIMessageBox:OnClickOK()
    self.confirmCallback()
    OnClose()
end

function UIMessageBox:OnClickCancel()
    self.cancelCallback()
    OnClose()
end

return UIMessageBox