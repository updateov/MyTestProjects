%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/getMeanLength
%
%   Description:    
%	   returns the average length of parts in the fhrPartSet
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%       meanLen             (double)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function meanLen = getMeanLength(this)

    numParts = length(this);
    totLen = calcTotalLength(this);
    
    
    meanLen = totLen / numParts;
    
return

