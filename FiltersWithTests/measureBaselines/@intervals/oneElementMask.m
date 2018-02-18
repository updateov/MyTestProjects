%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/oneElementMask
%
%   Description:    
%	   returns the intervals that remains in interval1
%      after removing any portions that intersect with 
%      interval2
%
%	 Parameters:
%	    intervals1 	      (intervals) assumed to be one-element
%	    intervals2        (intervals) assumed to be one-element
% 
%   Returns:
%       out               (intervals) 
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = oneElementMask(interval1, interval2)
% this is the case that needs to be split:
% 1 --------------
% 2   ----------
% ======>     
% 1 --          --            
% 2   ---------- 

out = empty(interval1);
if (interval1.x1 < interval2.x1)
    mask = intervals(interval1.x1, interval2.x1 - 1);
    out = add(out, mask);
end
if (interval1.x2 > interval2.x2)
    mask = intervals(interval2.x2 + 1, interval1.x2);
    out = add(out, mask);
end
return;
