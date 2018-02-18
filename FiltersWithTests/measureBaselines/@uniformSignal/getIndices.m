%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniform/getIndices
%
%   Description:    
%		accessor function for this.indices
%
%   Parameters:
%		this  (extrema)
%		
%   Returns:
%     indices (double)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function indices = getIndices(this, limits)
indices = 1:length(getSamples(this));
return