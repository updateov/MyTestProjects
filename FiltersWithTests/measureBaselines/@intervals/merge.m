%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/merge
%
%   Description:    
%		merges  the specified intervals to an intervals. Duplicates appear
%		only once (set union)
%
%   Parameters:
%		this
%		interval2		(intervals) the intervals to merge to this
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = merge(this, interval2)
thisPairs = [this.x1 ; this.x2]';
otherPairs = [get(interval2, 'x1') ; get(interval2, 'x2')]';
if isempty(thisPairs)
    this = interval2;
elseif ~isempty(otherPairs)
    unionPairs = union(thisPairs, otherPairs, 'rows');
    this.x1 = unionPairs(:, 1)';
    this.x2 = unionPairs(:, 2)';
%else interval2 isempty, do nothing
end
return
