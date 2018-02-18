%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/totalLength
%
%   Description:    
%		returns the sum of lengths of all intervals
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
function val = totalLength(this)
    val = 0;
    for i = 1:size(this)
        val = val + length(this, i);
    end
return;

