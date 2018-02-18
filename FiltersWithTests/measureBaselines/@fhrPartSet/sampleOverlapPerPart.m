%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/sampleOverlapPerPart
%
%   Description:    
%	   For each interval of set2, calculates the number of samples overlapping
%      with any interval(s) of set1
%
%	 Parameters:
%       set1              (fhrPartSet)  
%       set2              (fhrPartSet)  
%
%   Returns:
%       pcOverlap           (double)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function overlap = sampleOverlapPerPart(set1, set2)
OVERLAP_FACTOR = 0;
set2Intervals = toIntervals(set2);
% create single sample intervals for all samples of set1
set1Extents = [];
for i = 1:length(set1)
    curPart = set1.set{i};
    samples = getX1(getInterval(curPart)):getX2(getInterval(curPart));
    set1Extents = [set1Extents samples];
end
set1Extents = unique(set1Extents);
set1Intervals = intervals(set1Extents, set1Extents);
% for each interval in set2, determine overlap
overlap = zeros(1, length(set2));
for i = 1:length(set2)
    curPart = set2.set{i};
    [set1Out, set2Overlap, set2NonOverlap] = intersect(set1Intervals, getInterval(curPart), OVERLAP_FACTOR);
    overlap(i) = size(set1Out)/length(curPart);
end
return

