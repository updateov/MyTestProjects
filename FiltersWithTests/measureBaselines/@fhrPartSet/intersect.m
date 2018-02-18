%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/intersect
%
%   Description:    
%	   finds the intersection between two fhrPartSets
%
%	 Parameters:
%      fhrPartSet1       (fhrPartSet)  
%      fhrPartSet2       (fhrPartSet)  
%      USE_BIGGER_AS_DENOM: optional boolean flag to use the bigger
%                           interval as the denominator when calculating overlap percentage
%
%   Returns:
%     overlapIntervals1   (intervals) the intervals of fhrPartSet1 in the intersection 
%                         of fhrPartSet1 and fhrPartSet2
%     overlapIntervals2   (intervals) the intervals of fhrPartSet2 in the 
%                         intersection of fhrPartSet1 and fhrPartSet2
%     nonOverlapIntervals2 (intervals) the intervals of fhrPartSet2 not in the 
%                         intersection of fhrPartSet1 and fhrPartSet2
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [overlapIntervals1, overlapIntervals2, nonOverlapIntervals2] = intersect(fhrPartSet1, fhrPartSet2, overlapFactor, USE_BIGGER_AS_DENOM)

if(~(exist('USE_BIGGER_AS_DENOM')))
    USE_BIGGER_AS_DENOM = 0;
end
intervals1 = toIntervals(fhrPartSet1);
intervals2 = toIntervals(fhrPartSet2);
[overlapIntervals1 overlapIntervals2 nonOverlapIntervals2] = intersect(intervals1, intervals2, overlapFactor, USE_BIGGER_AS_DENOM);
return