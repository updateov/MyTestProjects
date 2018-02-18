%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/scale
%
%   Description:    
%       scale the intervals the specified factor
%   
%   Parameters:
%		this    (intervals) 
%		x1		(double) the x1 threshold
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = scale(this, factor)
this.x1 = floor(this.x1 * factor);
this.x2 = floor(this.x2 * factor);
return
