function [this fHandle] = openFigure(this, filename, name)
hashedName = doHashFunction(this, name);
if isfield(this.nameHash, hashedName);
    fHandle = this.nameHash.(hashedName);
    %the second was 100 times faster from command line
    %figure(fHandle);
    set(0,'CurrentFigure',fHandle)
else
    open(filename);
    fHandle = gcf;
    set(fHandle, 'Name', name);
    if this.doDocking
        set(fHandle,'WindowStyle','docked');    
    end
    this.nameHash.(hashedName) = fHandle;
end
return

