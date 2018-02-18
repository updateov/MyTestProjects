%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/unique
%
%   Description:    
%		Finds the sorted unique set of the specified fhrPartSet. 
%
%   Parameters:
%		this		(fhrPartSet) 
%
%   Returns:
%       this 	    (fhrPartSet)
%       iIndices    (double) indices of input in the output
%       jIndices    (double) indices of output in the input
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [this, iIndices, jIndices] = unique(this)
DO_SORT = false;
thisIntervals = toIntervals(this, DO_SORT);
[uniqueIntervals iIndices jIndices] = unique(thisIntervals);
%[[1:length(iIndices)]' iIndices [1:length(iIndices)]'==iIndices];
this.set = this.set(iIndices);
return;