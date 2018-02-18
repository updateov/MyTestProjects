%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/filterByX1greaterThan
%
%   Description:    
%	   filter out any parts whose x1 is greater than specified cutoff
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
function this = filterByX1greaterThan(this, x1)
toDeleteIndices = [];
for i = 1:length(this.set)
    if getX1(this.set{i}) > x1
        toDeleteIndices(end+1) = i;
    end
end
this = fhrPartSet(this.set{setdiff(1:length(this), toDeleteIndices)});
return