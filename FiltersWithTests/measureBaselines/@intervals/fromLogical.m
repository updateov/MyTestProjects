%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/fromUniformSignal
%
%   Description:
%		creates intervals from uniform signals for value at 1
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
function this = fromLogical(this, inLogical)
inLogical = inLogical(:)';
VALUE = 1;
inLogical = [1-inLogical(1) inLogical 1-inLogical(end)];
isInInterval = inLogical == VALUE;

diffIsInInterval = diff(isInInterval);
beginI = find(diffIsInInterval==1);
endI = find(diffIsInInterval==-1)-1;
if isempty(beginI) || isempty(endI)
    return
end
if beginI(1) > endI(1)
    endI(1) = [];
end
if isempty(endI)
    return
end
if endI(end) < beginI(end)
    beginI(end) = [];
end
if isempty(beginI)
    return
end
if length(beginI) ~= length(endI)
    error('Bad interval creation');
end
for iInterval = 1:length(beginI)
    this = add(this, intervals(beginI(iInterval), endI(iInterval)));
end
return