%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/closestX2LessThan
%
%   Description:    
%		finds the closest intervals whose x2 is less than to the specified
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
function [this, indices] = closestX2LessThan(this, x2)
indices = max(find(this.x2 < x2));
this = subset(this, indices);
return
