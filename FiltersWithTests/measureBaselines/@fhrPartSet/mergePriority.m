%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/mergePriority
%
%   Description:    
%	   returns the merge of all parts in fhrPartSet1 
%      with any parts in sets of fhrPartSet2.  If an overlap occurs,
%      the resulting merge is the result of writing over fhrPartSet1 with
%      the (priority) fhrPartSet2.
%
%	 Parameters:
%	    fhrPartSet1 	        (fhrPartSet) 
%	    fhrPartSet2             (fhrPartSet) the priority fhrPartSet 
%	    overlapFactor           (double) 
% 
%   Returns:
%       out                     (fhrPartSet)
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = mergePriority(fhrPartSet1, fhrPartSet2)
fhrPartSet1Mask = mask(fhrPartSet1, fhrPartSet2);
out = union(fhrPartSet1Mask, fhrPartSet2);
return
