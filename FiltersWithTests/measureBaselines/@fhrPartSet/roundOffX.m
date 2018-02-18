%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/roundOffX1
%
%   Description:    
%		rounding off the X1 values
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%       this              (fhrPartSet)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = roundOffX(this)
for i = 1:length(this)
    curPart = this.set{i};
    curPart = roundOffX(curPart);
    this.set{i} = curPart;
end
return

