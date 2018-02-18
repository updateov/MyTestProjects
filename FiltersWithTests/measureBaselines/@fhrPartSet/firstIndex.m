%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/firstIndex
%
%   Description:    
%	   returns the first index of the first part of this
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%       index             (double)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function index = firstIndex(this)
if isempty(this)
    index = [];
else
    index = getX1(getInterval(this.set{1}));
end
return

