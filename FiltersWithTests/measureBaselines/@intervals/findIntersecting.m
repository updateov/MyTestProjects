%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/findIntersecting
%
%   Description:    
%		return intervals that overlap.  Results will be sorted
%
%   Parameters:
%		this 				(interval)
%       overlapThresh       (double)  The minimum overlap to be considered 
%                                      intersecting
%
%   Returns:
%     intersectI    The first of the pairs of sorted intersecting intervals
%
%	History:
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [this intersectI] = findIntersection(this, overlapThreshold)
if nargin < 2
    overlapThreshold = 1;
end    
this = sort(this);
gaps = this.x1(2:end) - this.x2(1:end-1);
intersectI = find(gaps < overlapThreshold);
return

