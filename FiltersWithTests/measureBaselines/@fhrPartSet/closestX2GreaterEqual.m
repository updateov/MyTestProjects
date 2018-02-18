%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/closestX2GreaterEqual
%
%   Description:    
%		finds the closest fhrPartSet whose x2 is greater than or equal to the specified
%		x2 value
%
%   Parameters:
%		this    (fhrPartSet) 
%		x2		(double) the x2 threshold
%
%   Returns:
%       out		(fhrPart)
%       partIndex   (double) the index of the requested part in this
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [out, partIndex] = closestX2GreaterEqual(this, x2)
thisIntervals = toIntervals(this);
[thisIntervals partIndex] = closestX2GreaterEqual(thisIntervals, x2);
if isempty(partIndex)
    out = [];
    return;
end
out = this.set{partIndex};
return
