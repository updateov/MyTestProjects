%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/mask
%
%   Description:    
%	   returns the parts that remain in fhrPartSet1 after removing any 
%      portions that intersect with any parts in sets of fhrPartSet2.
%
%	 Parameters:
%	    fhrPartSet1 	        (fhrPartSet) the priority fhrPartSet
%	    fhrPartSet2             (fhrPartSet) 
% 
%   Returns:
%       out                     (fhrPartSet)
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = mask(fhrPartSet1, fhrPartSet2)
out = fhrPartSet;
intervals2 = toIntervals(fhrPartSet2);
for i=1:length(fhrPartSet1.set)
    intervals1 = getInterval(fhrPartSet1.set{i});
    intervals1 = mask(intervals1, intervals2);
    for j=1:size(intervals1)
        outPart = setInterval(fhrPartSet1.set{i}, subset(intervals1, j));
        out = add(out, outPart);
    end
 end
return
