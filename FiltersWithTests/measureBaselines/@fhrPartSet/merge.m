%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/merge
%
%   Description:    
%		merges  the specified intervals to an intervals. Orders according to 
%       x1 values. Duplicates appear only once (set union)
%
%	 Parameters:
%       fhrPartSet1     (fhrPartSet)  
%       fhrPartSet2     (fhrPartSet)  
%
%   Returns:
%       out             (fhrPartSet)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = merge(fhrPartSet1, fhrPartSet2)
%pairs1 = [this.x1 ; this.x2]';
if isempty(fhrPartSet1)
    out = fhrPartSet2;
    return
end
if isempty(fhrPartSet2)
    out = fhrPartSet1;
    return
end
out = fhrPartSet1;
set2 = getRawData(fhrPartSet2);
for i = 1:length(set2)
    out = merge(out, getInterval(set2{i}));
end
return

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