%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/intersectPercent
%
%   Description:    
%	   finds the intersection between two fhrPartSets
%
%	 Parameters:
%      fhrPartSet1       (fhrPartSet)  
%      fhrPartSet2       (fhrPartSet)
%      USE_BIGGER_AS_DENOM: optional boolean flag to use the bigger
%                           interval as the denominator when calculating
%                           overlap percentage
%
%   Returns:
%     overlapPercent     (double) for each interval in intervals2, the 
%                                 percentage overlap
%     overlapIndices     (double) n indices of this that overlap interval2.
%                                 An index of -1 indicates no overlap.  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [overlapPercent, overlapIndices] = ...
    intersectPercent(fhrPartSet1, fhrPartSet2, USE_BIGGER_AS_DENOM)
if(~(exist('USE_BIGGER_AS_DENOM')))
    USE_BIGGER_AS_DENOM = 0;
end

intervals1 = toIntervals(fhrPartSet1);
intervals2 = toIntervals(fhrPartSet2);
[overlapPercent overlapIndices] = ...
    intersectPercent(intervals1, intervals2, USE_BIGGER_AS_DENOM);
return