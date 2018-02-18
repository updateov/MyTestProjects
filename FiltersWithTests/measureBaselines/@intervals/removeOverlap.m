%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/removeOverlap
%
%   Description:    
%       merge overlapping intervals into one
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
function this = removeOverlap(this)
minX1 = min(this.x1);
maxX2 = max(this.x2);

maskInterval = intervals(minX1, maxX2);
this = mask(maskInterval, this);
this = mask(maskInterval, this);
return

