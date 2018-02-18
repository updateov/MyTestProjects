%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/findNextByType
%
%   Description:    
%       Finds the fhrPart following the given index with the given type
%
%   Parameters:
%		this		(fhrPartSet) 
%       index       (double)
%       className   (char) the name of an fhrPart class
%
%   Returns:
%       indices     (double) indices of the matching fhrParts
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function anFhrPart = findNextByType(this, index, className)
anFhrPart = [];
for iIndex = index+1:length(this.set)
    curPart = this.set{iIndex};
    if isa(curPart, className)
        anFhrPart = curPart;
        return;
    end
end
return
