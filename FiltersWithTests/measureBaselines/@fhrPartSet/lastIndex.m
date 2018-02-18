%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/lastIndex
%
%   Description:    
%	   returns the last index of the last part of this
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
function index = lastIndex(this)
if isempty(this)
    index = [];
else
    index = getX2(getInterval(this.set{end}));
end
return

