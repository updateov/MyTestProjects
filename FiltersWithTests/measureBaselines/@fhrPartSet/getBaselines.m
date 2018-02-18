%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/getBaselines
%
%   Description:    
%	   returns only the elements of this of the specified class
%
%	 Parameters:
%       this              (fhrPartSet)  
%       className         (char)  the name of the subclass 
%
%   Returns:
%       baselines         (fhrPartSet) 
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = getBaselines(this)
out = filterByType(this, 'baseline');
return

