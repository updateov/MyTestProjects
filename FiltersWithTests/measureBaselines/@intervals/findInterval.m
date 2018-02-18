%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/findInterval
%
%   Description:    
%	   returns the index of the interval matching the specified interval
%
%	 Parameters:
%	    this 	          (intervals) 
%	    intervals2        (intervals) must have one element only
% 
%   Returns:
%       indices           (double) the indices of the intervals matching
%                                  the input interval 
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function indices = findInterval(this, intervals2)
x1 = get(intervals2, 'x1');
x2 = get(intervals2, 'x2');

if (length(x1) > 1 | length(x2) > 1)
    error('input interval must be single');
end

indices = find(x1 == this.x1 & ... 
               x2 == this.x2);
return