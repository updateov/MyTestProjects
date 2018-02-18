%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/x1LessEqual
%
%   Description:    
%		finds intervals whose x1 is less than or equal to the specified
%		x1 value
%
%   Parameters:
%		this    (intervals) 
%		x1		(double) the x1 threshold
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = x1LessEqual(this, x1)
indices = find(this.x1 <= x1);
this = subset(this, indices);
return
