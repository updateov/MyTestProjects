%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/getMidPoint
%
%   Description:    
%		returns the interval midpoint (x1 coordinate)
%
%	 Signatures
%		interval = getMidPoint(this)
%
%   Parameters
%       this (intervals)
%
%   Returns:
%       midPoint (double)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function midPoint = getMidPoint(this)
midPoint = (this.x1 + this.x2)/2;
return
