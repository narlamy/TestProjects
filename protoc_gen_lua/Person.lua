ReqPerson = {
  Header = {
    Category=0  
}, ID=""
}

ResPerson = {
  ErrCode=0, Age=0, Name=""
}

_ReqHeader = {
  Category=0
}

_ResHeader = {
  ErrCode=0
}

function ReqPerson:SetID(id)
  self.ID=id;
  return self
end
function ReqPerson:Send(onRes)
  CS.N2.Network.LuaSender.Send(self)
end
function ReqPerson:GetIndex(fieldName)
  if fieldName == "Header" then return 1;
  if fieldName == "ID" then return 10;
}
end
ReqPerson.Res=ResPerson
function ReqPerson:new()
  return setmetatable(self or {}, {_index=ReqPerson})
end

return ReqPerson.new()