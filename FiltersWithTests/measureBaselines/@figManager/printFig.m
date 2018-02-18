function printFig(this, prefix, suffix)
if nargin < 3
    suffix = '';
end
if nargin < 2
    prefix = '';
end
if isempty(this.nameHash)
    return
end
fnames = fieldnames(this.nameHash);
for iFigure = 1:length(fnames)
    fHandle = this.nameHash.(fnames{iFigure});
    %figure(fHandle);
    name = get(fHandle, 'Name');
    %saveas(fHandle, [name '.fig'], 'fig');
    %printFig(fHandle, [name '.fig']);
    printFig(fHandle, [prefix name suffix '.fig']);
end
return