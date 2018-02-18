
function printPdf(this, dirBase, prefix, suffix)
if nargin < 4
    suffix = '';
end
if nargin < 3
    prefix = '';
end
if nargin < 2
    dirBase = '.';
end
if isempty(this.nameHash)
    return
end
fnames = fieldnames(this.nameHash);
for iFigure = 1:length(fnames)
    fHandle = this.nameHash.(fnames{iFigure});
    %figure(fHandle);
    set(0,'CurrentFigure',fHandle)
    name = get(fHandle, 'Name');
    name = strrep(name, ' ', '_');
    printPdf(fHandle, [dirBase '\\' prefix name suffix '.pdf']);
end
return
