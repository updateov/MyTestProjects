%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/length
%
%   Description:    
%		returns the length of the interval at index i
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
function val = length(this, i)
if isempty(this.x2) | isempty(this.x1)
    val = 0;
    return
end
if nargin == 1
    i = 1;
end
val = this.x2(i) - this.x1(i) + 1;
return