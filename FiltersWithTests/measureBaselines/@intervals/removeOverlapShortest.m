%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/removeOverlapShortest
%
%   Description:    
%       for overlapping intervals, keep longest interval
%		
%
%   Parameters:
%		this 				(interval)
%
%   Returns:
%     none
%
%	History:
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [this deleteI] = removeOverlapShortest(this, overlapThreshold)
[dummy intersectI] = findIntersecting(this, overlapThreshold);
% for overlapping features, keep the one that is longest
deleteI = [];
for i = 1:(length(intersectI)-1)
    [minVal minI] = min([length(subset(this, intersectI(i))) ...
                         length(subset(this, intersectI(i+1)))]);
    deleteI = [deleteI intersectI(i)+minI-1];
end
this = delete(this, deleteI);
return

