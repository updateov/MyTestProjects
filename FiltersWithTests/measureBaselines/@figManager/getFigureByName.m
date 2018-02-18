function [this fHandle] = getFigureByName(this, name)
%hashedName = ['h' num2str(sum(name*name'))];
%hashedName = ['h' num2str(sum(int64(name).*int64(((1:length(name)).^3))))];
hashedName = doHashFunction(this, name);
if isfield(this.nameHash, hashedName);
    fHandle = this.nameHash.(hashedName);
    %the second was 100 times faster from command line
    %figure(fHandle);
    set(0,'CurrentFigure',fHandle)
else
    fHandle = figure;
    set(fHandle, 'Name', name);
    if this.doDocking
        set(fHandle,'WindowStyle','docked');    
    end
    this.nameHash.(hashedName) = fHandle;
    %is this required?
    %figure(fHandle);
end
return
