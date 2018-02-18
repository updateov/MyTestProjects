%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/filterByLength
%
%   Description:    
%	   filters out any parts shorter than specified minimum length
%
%	 Parameters:
%       this              (fhrPartSet)  
%       minLength         (double)
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = filterByHeight(this, minHeight, maxHeight)
indices = zeros(1, 10000);
nOutElements = 0;
if ~exist('maxHeight', 'var')
    maxHeight = inf;
end
for i = 1:length(this.set)
    keep = 0;
    if (isa(this.set{i}, 'bump')) || (isa(this.set{i}, 'bump2'))
        if (getHeight(this.set{i}) >= minHeight) && (getHeight(this.set{i}) < maxHeight)
            keep = 1;
        end
    else
        keep = 1;
    end
    if keep
        nOutElements = nOutElements + 1;
        indices(nOutElements) = i;
    end
end
indices(nOutElements + 1:end) = [];
this = fhrPartSet(this.set{indices});
if nargout == 2
    inIndices = indices;
end
return

