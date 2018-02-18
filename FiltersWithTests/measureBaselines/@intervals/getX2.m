%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/getX2
%
%   Description:    
%		accessor function for the second coordinate at index i
%
%   Parameters:
%		this  (intervals)
%		
%   Returns:
%     val
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function val = getX2(this, i)
if nargin == 1
    val = this.x2;
else
    val = this.x2(i);
end
return