%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/filterShort
%
%   Description:    
%		removes intervals with length less than the given
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
function [this indices] = filterShort(this, thresh)
indices = find(this.x2-this.x1+1 < thresh);
this.x1(indices) = [];
this.x2(indices) = [];
return