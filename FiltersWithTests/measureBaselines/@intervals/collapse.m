%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/collapse
%
%   Description:    
%		Collapses the intervals from iBegin to iEnd into one interval
%       setting x1 to iBegin.x1 and x2 to iEnd.x2
%
%   Parameters:
%		this    	   (intervals) 
%
%   Returns:
%       this            (intervals) 
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = collapse(this, iBegin, iEnd)
this.x1 = [this.x1(1:iBegin)   this.x1(iEnd+1:end)];
this.x2 = [this.x2(1:iBegin-1) this.x2(iEnd:end)];
return
