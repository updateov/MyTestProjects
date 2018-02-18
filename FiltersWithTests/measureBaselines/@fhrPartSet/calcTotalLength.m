%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/calcTotalLength
%
%   Description:    
%	   Calculates the total length of all individual fhrPart
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function totalLength = calcTotalLength(this)
totalLength = 0;
for i = 1:length(this.set)
    totalLength = totalLength + length(this.set{i});
end
return

