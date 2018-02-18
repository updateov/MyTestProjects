%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/removeByType
%
%   Description:    
%	   returns only the elements of this of the specified class
%
%	 Parameters:
%       this              (fhrPartSet)  
%       className         (char)  the name of the subclass 
%
%   Returns:
%       baselines         (fhrPartSet) 
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = removeByType(this, className)
indices = zeros(1, 10000);
nOutElements = 0;
for i = 1:length(this.set)
    if ~isa(this.set{i}, className)
        nOutElements = nOutElements + 1;
        indices(nOutElements) = i;
    end
end
indices(nOutElements + 1:end) = [];
out = fhrPartSet(this.set{indices});
return

