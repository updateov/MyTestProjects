%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/x2LessThanEqual
%
%   Description:    
%		finds intervals whose x2 is greater than or equal to the specified
%		x2 value
%
%   Parameters:
%		this    (intervals) 
%		x2		(double) the x2 threshold
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = x2LessThanEqual(this, x2)
indices = find(this.x2 <= x2);
this = subset(this, indices);
return
