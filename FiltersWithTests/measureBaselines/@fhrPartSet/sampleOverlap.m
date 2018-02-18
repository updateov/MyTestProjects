%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/sampleOverlap
%
%   Description:    
%	   Calculates the overlap of set2 with set1.  That is, for all
%      the samples of all intervals in this, how many are also in
%      intervals of set2
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
function overlap = sampleOverlap(set1, set2)
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
% determine overlap
[set1Out, set2Overlap, set2NonOverlap] = intersect(set1Intervals, set2Intervals, 0);
overlap = size(set1Out);
return

