%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/add
%
%   Description:    
%	   removes the interval at the specified index from the interval set
%
%	 Parameters:
%       this              (fhrPartSet)  
%	    indices        	  (double) 
%
%   Returns:
%       this	          (fhrPartSet)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = remove(this, indices)
this.set(indices) = [];
return

