%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/sort
%
%   Description:    
%		Finds the sorted unique set for the specified intervals.
%
%   Parameters:
%		this    	   (intervals) 
%
%   Returns:
%       this            (intervals) 
%       indices        (double) indices of input in the output
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [this, indices] = sort(this)

[dummy, indices] = sort(this.x1);
this.x1 = this.x1(indices);
this.x2 = this.x2(indices);

return
