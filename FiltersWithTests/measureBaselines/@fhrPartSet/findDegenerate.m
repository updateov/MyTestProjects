%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/findDegenerate
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
function [degenIndices, degenLength, reasons] = findDegenerate(this, fhrBuffer)
degenIndices = [];
degenLength = [];
reasons = {};
for i = 1:length(this.set)
    [currIsDegenerate reason] = isDegenerate(this.set{i}, fhrBuffer);
    if currIsDegenerate
        degenIndices = [degenIndices i];
        degenLength = [degenLength length(this.set{i})];
        reasons{end+1} = reason;
    end
end
return


 