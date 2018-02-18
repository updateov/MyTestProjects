function hashedName = doHashFunction(this, name)
hashedName = ['h' num2str(sum(int32(name).*int32(((1:length(name)).^3))))];
return
