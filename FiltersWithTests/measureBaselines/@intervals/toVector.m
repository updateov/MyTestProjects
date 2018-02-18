%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/toVector
%
%   Description:    
%		returns the interval at index i as a 2-element vector 
%
%   Parameters:
%		this  (intervals)
%		
%   Returns:
%     vec     (2-element vector)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function vec = toVector(this)
if nargin == 1
    i = 1;
end
vec = [this.x1(i) this.x2(i)];
return