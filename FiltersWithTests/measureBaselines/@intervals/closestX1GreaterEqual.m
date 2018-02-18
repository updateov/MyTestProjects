%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/closestX1LessEqual
%
%   Description:    
%		finds the closest intervals whose x1 is less than or equal to the specified
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
function [this, indices] = closestX1GreaterEqual(this, x1)
indices = min(find(this.x1 >= x1));
this = subset(this, indices);
return
