%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/findOverlap
%
%   Description:    
%	   finds overlapping fhrParts
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [overlapIndices, overlapSamples] = findOverlap(this)
overlapIndices = [];
overlapSamples = [];
for i = 1:length(this.set)
    if (i > 1) 
        currOverlapSamples = getX2(this.set{i-1}) - getX1(this.set{i});
        if currOverlapSamples > 0
            overlapIndices = [overlapIndices i];
            overlapSamples = [overlapSamples currOverlapSamples];
        end
    end
end
return


