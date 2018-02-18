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
function this = filterByLength(this, minLength, maxLength)
indices = zeros(1, 10000);
nOutElements = 0;
if ~exist('maxLength', 'var')
    maxLength = inf;
end
for i = 1:length(this.set)
    if (length(this.set{i}) >= minLength) && (length(this.set{i}) < maxLength)
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

