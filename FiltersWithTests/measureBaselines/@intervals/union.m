%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/union
%
%   Description:    
%		Finds the sorted aset union of the specified intervals. Duplicates appear
%		only once.
%
%   Parameters:
%		intervals1		(intervals) 
%		intervals2		(intervals) 
%
%   Returns:
%       intervals1 	    (intervals)
%       indices1        (double) indices of interval1 in the out
%       indices2        (double) indices of interval2 in the out
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [intervals1, indices1, indices2] = union(intervals1, intervals2)
intervalsPairs1 = [intervals1.x1 ; intervals1.x2]';
intervalsPairs2 = [get(intervals2, 'x1') ; get(intervals2, 'x2')]';
if isempty(intervalsPairs1)
    this = interval2;
elseif ~isempty(intervalsPairs2)
    [unionPairs indices1 indices2] = union(intervalsPairs1, intervalsPairs2, 'rows');
    intervals1.x1 = unionPairs(:, 1)';
    intervals1.x2 = unionPairs(:, 2)';
%else interval2 isempty, do nothing
end
return
