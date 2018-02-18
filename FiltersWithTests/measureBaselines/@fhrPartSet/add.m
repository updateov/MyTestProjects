%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/add
%
%   Description:    
%	   adds an fhrPart(s) to the set
%
%	 Parameters:
%       this              (fhrPartSet)  
%	    fhrParts    	  (fhrPart or fhrPartSet) 
%
%   Returns:
%       this	          (fhrPartSet)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = add(this, fhrParts)
if isa(fhrParts, 'fhrPart')
    this.set{end + 1} = fhrParts;
elseif isa(fhrParts, 'fhrPartSet')
    nElements = length(fhrParts);
    this.set(end + 1: end + nElements) = getRawData(fhrParts);
end
return
    
