%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/removeOverlap
%
%   Description:    
%       merge overlapping intervals into adjacent non-overlapping intervals
%		
%
%   Parameters:
%		this 				(interval)
%
%   Returns:
%     none
%
%	History:
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = removeOverlapDistinct(this, overlapThreshold)
[dummy intersectI] = findIntersecting(this, overlapThreshold);
for i = 1:(length(intersectI)-1)
    this.x1(intersectI+1) = this.x2(intersectI)+1;
end
return

