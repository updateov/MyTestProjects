function this = clearAll(this)
if isempty(this.nameHash)
    return
end
fnames = fieldnames(this.nameHash);
for iFigure = 1:length(fnames)
    fHandle = this.nameHash.(fnames{iFigure});
    %if isfloat(fHandle)
    if isa(fHandle, 'matlab.ui.Figure')        
        figure(fHandle);
        clf;
    end
end
return
