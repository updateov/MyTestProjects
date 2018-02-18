%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/translateX2
%
%   Description:    
%       translate x2 forward by the indicated amount
%   
%   Parameters:
%		this    (intervals) 
%		transl	(double) 
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = translateX2(this, transl)
this.x2 = this.x2 + transl;
return
