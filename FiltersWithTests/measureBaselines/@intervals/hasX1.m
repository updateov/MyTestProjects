%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/hasX1
%
%   Description:    
%		returns whether any intervals have the specified x1 value
%
%   Parameters:
%		this  (intervals)
%		x1  
%		
%   Returns:
%     flag   (boolean)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function flag = hasX1(this, x1)
flag = ~isempty(find(this.x1 == x1));
return