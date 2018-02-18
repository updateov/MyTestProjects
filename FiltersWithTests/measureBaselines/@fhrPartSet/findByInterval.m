%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/findByInterval
%
%   Description:    
%       Finds the fhrParts having intervals that match the input interval
%
%   Parameters:
%		this		(fhrPartSet) 
%		intervals2	(intervals) 
%
%   Returns:
%       indices     (double) indices of the matching fhrParts
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function indices = findByInterval(this, intervals2)
thisIntervals = toIntervals(this);
indices = findInterval(thisIntervals, intervals2);
return
