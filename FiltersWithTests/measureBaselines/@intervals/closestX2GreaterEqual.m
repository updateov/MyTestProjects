%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/closestX2GreaterEqual
%
%   Description:    
%		finds the closest intervals whose x2 is greater than or equal to the specified
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
function [this, indices] = closestX2GreaterEqual(this, x2)
indices = min(find(this.x2 >= x2));
this = subset(this, indices);
return
