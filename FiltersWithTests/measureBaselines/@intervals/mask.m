%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/mask
%
%   Description:    
%	   returns the intervals that remains in intervals1
%      after removing any portions that intersect with 
%      intervals in intervals2
%
%	 Parameters:
%	    intervals1 	      (intervals) 
%	    intervals2        (intervals) 
% 
%   Returns:
%       out               (intervals) 
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = mask(intervals1, intervals2)
OVERLAP_FACTOR = 1.0E-09;
out = intervals1;
for i=1:size(intervals2)
    iInterval = subset(intervals2, i);
    [overlap out] = oneElementIntersect(out, iInterval, OVERLAP_FACTOR);
    for j=1:size(overlap)
        jInterval = subset(overlap, j);
        maskIntervals = oneElementMask(jInterval, iInterval);
        out = merge(out, maskIntervals);
    end
end
return

