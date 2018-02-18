%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/filterByX1
%
%   Description:    
%	   filter out any parts whose x1 is less than the specified minX1
%
%	 Parameters:
%       this              (fhrPartSet)  
%       minX1             (double)
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = filterByX1(this, minX1)
toDeleteIndices = [];
for i = 1:length(this.set)
    if getX1(this.set{i}) < minX1
        toDeleteIndices(end+1) = i;
    end
end
this = fhrPartSet(this.set{setDiff(1:length(this), toDeleteIndices)});
return

