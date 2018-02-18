%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/setFhrPartParser
%
%   Description:    
%		setter for the fhrPartParser object
%
%	 Signatures
%		this = setFhrPartParser(this, parser)
%
%   Parameters
%       this    (fhrPart)
%       parser  (fhrPartParser)
%
%   Returns:
%       this    (fhrPart)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = setFhrPartParser(this, parser)
this.fhrPartParser = parser;
return
