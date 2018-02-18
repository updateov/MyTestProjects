%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/sort
%
%   Description:    
%		Sorts fhrPartSet based on x1 
%
%   Parameters:
%		this		(fhrPartSet) 
%
%   Returns:
%       this 	    (fhrPartSet)
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [this] = sort(this)

numParts = length(this);
x1 = zeros(1, numParts);

for i = 1:length(this)
    x1(i) = getX1(this.set{i});
end

[sortX1, indices] = sort(x1);
    

this.set = this.set(indices);
return;