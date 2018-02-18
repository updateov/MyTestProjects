%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/setReferenceFhr
%
%   Description:    
%	   (Re)calculates the fhrParts that are dependent on the fhr value
%        (currently only baselines).
%
%	 Parameters:
%       this              (fhrPartSet)  
%       fhr               (double)  the vector of fhr samples
%
%   Returns:
%       pcOverlap           (double)  
%
%   Todo:                 replace fhr(double) with fhr(uniformSignal)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = setReferenceFhr(this, fhr)
for i = 1:length(this)
    curPart = this.set{i};
    curPart = setReferenceFhr(curPart, fhr);
    this.set{i} = curPart;
end
return

