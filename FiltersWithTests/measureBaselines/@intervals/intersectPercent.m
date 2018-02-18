%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/intersectPercent
%
%   Description:    
%	   For each interval in intervals2 determine the degree of overlap
%      with intervals this.  If there is overlap with more than one 
%      interval in this, choose the greatest overlap.  
%
%	 Parameters:
%	    this 	          (intervals) 
%	    intervals2        (intervals) 
%       USE_BIGGER_AS_DENOM: optional boolean flag to use the bigger
%                           interval as the denominator when calculating
%                           overlap percentage
% 
%   Returns:
%     overlapPercent      (double)    for each of the n elements in 
%                                     overlapIntervals, the percentage
%                                     overlap with an interval in this
%     overlapIndices      (double)    n indices of this that overlap interval2
%                                     An index of -1 indicates no overlap
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [overlapPercent, overlapIndices] = ...
    intersectPercent(this, intervals2, USE_BIGGER_AS_DENOM)

% consider any overlap
OVERLAP_FACTOR = 1E-12;  

% USE_BIGGER_INTERVAL_AS_DENOM is optional flag that can be set to use the
% bigger of the two intervals as the denom when calculating overlap perc. -
% default is that 'this' interval always used as denom - can be issue when
% interval2 is much larger.  Flag defaults to 0 for backwards compatibility
if (~(exist('USE_BIGGER_INTERVAL_AS_DENOM')))
    USE_BIGGER_INTERVAL_AS_DENOM = 0;
end

thisOut = intervals;
intervals2Overlap = intervals;
intervals2NonOverlap = intervals;
overlapPercent = zeros(1, size(intervals2));
overlapIndices = ones(1, size(intervals2)) * -1;

%nonOverlap = this;
for i=1:size(intervals2)
    iInterval = subset(intervals2, i);
    [currPercent currentIndices] = oneElementIntersectPercent(this, iInterval, OVERLAP_FACTOR, USE_BIGGER_AS_DENOM);
    if ~isempty(currPercent)
        % keep only the interval with best overlap
        [maxPercent maxIndex] = max(currPercent);
        overlapPercent(i) = maxPercent;
        overlapIndices(i) = currentIndices(maxIndex);
    else
        intervals2NonOverlap = add(intervals2NonOverlap, iInterval);
        % overlapPercent(i) = 0;
        % overlapIndices(i) = -1;
    end
end
return

