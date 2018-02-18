%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/isEmpty
%
%   Description:    
%		returns whether the number of intervals is zero
%
%   Parameters:
%		this  (intervals)
%		
%   Returns:
%       flag  (boolean)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function flag = isempty(this)
flag = size(this) == 0;
return;
