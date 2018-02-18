%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/delete
%
%   Description:    
%		removes intervals at given indices
%
%   Parameters:
%		this  (intervals)
%		thresh (double)
%		
%   Returns:
%     flag   (boolean)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = delete(this, indices)
this.x1(indices) = [];
this.x2(indices) = [];
return