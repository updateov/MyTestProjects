%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/translateAndScale
%
%   Description:    
%       translate and scales the intervals forward by the indicated amount
%   
%   Parameters:
%		this    (intervals) 
%		x1		(double) the x1 threshold
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = translateAndScale(this, transl, scale)
this.x1 = floor((this.x1-transl)/scale);
this.x2 = floor((this.x2-transl)/scale);
return
