%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/toLogical
%
%   Description:
%		converts the interval points to true; otherwise false;
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
function out = toLogical(this, totalLength)
out = zeros(1, totalLength);
for i = 1:length(this.x1)
    out(uint64(this.x1(i)):uint64(this.x2(i))) = 1;
end    
return