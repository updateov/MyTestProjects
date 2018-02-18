%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/subset
%
%   Description:    
%		extracts an intervals subset from this using specified indices
%
%   Parameters:
%		this
%		indices 		(intervals) the intervals to add to this
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = subset(this, indices)
this.x1 = this.x1(indices);
this.x2 = this.x2(indices);
return

