%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/getX1
%
%   Description:    
%		accessor function for the first coordinate at index i
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
function val = getX1(this, i)
if nargin == 1
    val = this.x1;
else
    val = this.x1(i);
end
return