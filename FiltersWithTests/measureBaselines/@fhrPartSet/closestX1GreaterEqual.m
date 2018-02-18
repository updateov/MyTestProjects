%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/closestX1LessEqual
%
%   Description:    
%		finds the closest fhrPartSet whose x1 is less than or equal to the specified
%		x1 value
%
%   Parameters:
%		this    (fhrPartSet) 
%		x1		(double) the x1 threshold
%
%   Returns:
%       out		(fhrPart)
%       partIndex   (double) the index of the requested part in this
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [out, partIndex] = closestX1GreaterEqual(this, x1)
thisIntervals = toIntervals(this);
[thisIntervals partIndex] = closestX1GreaterEqual(thisIntervals, x1);
if isempty(partIndex)
    out = [];
    return;
end
out = this.set{partIndex};
return
