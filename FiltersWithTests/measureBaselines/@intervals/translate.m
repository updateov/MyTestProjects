%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/translate
%
%   Description:    
%       translate the intervals forward by the indicated amount
%   
%   Parameters:
%		this    (intervals) 
%		x1		(double) the x1 threshold
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = translate(this, transl)
this.x1 = this.x1 + transl;
this.x2 = this.x2 + transl;
return
