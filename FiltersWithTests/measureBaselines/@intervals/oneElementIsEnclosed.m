%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/oneElementIsEnclosed
%
%   Description:    
%		determines if the intervals in this are enclosed by 
%       interval2
%
%   Parameters:
%		this  (intervals)
%       interval2  (intervals) - assumed to have one element only
%       overlap (double)
%		
%   Returns:
%       indices     - indices of intervals in this that are enclosed by interval2
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [indices] = oneElementIsEnclosed(this, interval2)

overlapIntervals = empty(this);

x1 = get(interval2, 'x1');
x2 = get(interval2, 'x2');

indices = find(x1 <= this.x1 & ...
               x2 >= this.x2);
           
overlapIntervals = add(overlapIntervals, subset(this, indices));   

return;

