%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/isempty
%
%   Description:    
%	   returns true if there are no elements in the set
%
%	 Parameters:
%       this              (fhrPartSet)  
%
%   Returns:
%       flag              (double)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function flag = isempty(this)
flag = isempty(this.set);
return

