%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/intersect
%
%   Description:    
%	   returns the intersection of all interval in this with all intervals
%      in intervals2.  An intersection is defined by overlapThreshold (double), 
%      the minimal amount of interval overlap 
%
%	 Parameters:
%	    this 	             (intervals) 
%	    intervals2           (intervals) 
%       USE_BIGGER_AS_DENOM:  optional boolean flag to use the bigger
%                             interval as the denominator when calculating
%                             overlap percentage
% 
%   Returns:
%     thisOut             (intervals) the intervals of this in the intersection 
%                         of this and intervals2
%     intervals2Overlap   (intervals) the intervals of intervals2 in the 
%                         intersection of this and intervals2
%     intervals2NonOverlap (intervals) the intervals of intervals2 in the 
%                         intersection of this and intervals2
%     overlapI            (double) indices of overlapping intervals in
%                         intervals2
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [thisOut, intervals2Overlap, intervals2NonOverlap overlapI] = intersect(this, intervals2, overlapFactor, USE_BIGGER_AS_DENOM)
thisOut = intervals;
intervals2Overlap = intervals;
intervals2NonOverlap = intervals;
overlapI = [];
% flag for using bigger interval as denom in any perc. overlap calculations
% default to 0 for backwards compatibility
if(~(exist('USE_BIGGER_AS_DENOM')))
    USE_BIGGER_AS_DENOM = 0;
end

nonOverlap = this;

for i=1:size(intervals2)
    iInterval = subset(intervals2, i);
    [overlap nonOverlap] = oneElementIntersect(nonOverlap, iInterval, overlapFactor, USE_BIGGER_AS_DENOM);
    if ~isempty(overlap)
        thisOut = merge(thisOut, overlap);
        intervals2Overlap = add(intervals2Overlap, iInterval);
        overlapI = [overlapI i];
    else
        intervals2NonOverlap = add(intervals2NonOverlap, iInterval);
    end
end
return

