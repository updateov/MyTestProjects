function this = closeAll(this)
if isempty(this.nameHash)
    return
end
fnames = fieldnames(this.nameHash);
for iFigure = 1:length(fnames)
    fHandle = this.nameHash.(fnames{iFigure});
    if ~isempty(fHandle)
        close(fHandle);
    end
end
this.nameHash = [];
return
