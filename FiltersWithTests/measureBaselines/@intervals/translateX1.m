%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/translateX1
%
%   Description:    
%       translate x1 forward by the indicated amount
%   
%   Parameters:
%		this    (intervals) 
%		x1		(double) the x1 threshold
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = translateX1(this, transl)
this.x1 = this.x1 + transl;
return
