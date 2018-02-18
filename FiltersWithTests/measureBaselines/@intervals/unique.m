%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/unique
%
%   Description:    
%		Finds the sorted unique set for the specified intervals.
%
%   Parameters:
%		this    	   (intervals) 
%
%   Returns:
%       this            (intervals) 
%       iIndices        (double) indices of input in the output
%       jIndices        (double) indices of output in the input
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [this, iIndices, jIndices] = unique(this)
if isempty(this)
    iIndices = [];
    jIndices = [];
    return
end
thisPairs = [this.x1 ; this.x2]';
[uniquePairs iIndices jIndices] = unique(thisPairs, 'rows');
this.x1 = uniquePairs(:, 1)';
this.x2 = uniquePairs(:, 2)';
return
