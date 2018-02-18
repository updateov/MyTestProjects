%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/union
%
%   Description:    
%		Finds the sorted aset union of the specified fhrPartSet. Duplicates appear
%		only once.
%
%   Parameters:
%		fhrPartSet1		(fhrPartSet) 
%		fhrPartSet2		(fhrPartSet) 
%
%   Returns:
%       fhrPartSet1 	(fhrPartSet)
%       iIndices        (double) indices of fhrPartSet1 in the union
%       jIndices        (double) indices of fhrPartSet2 in the union
%               
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [fhrPartSet1, iIndices, jIndices] = union(fhrPartSet1, fhrPartSet2)
DO_MERGE = false;
if isempty(fhrPartSet1) & isempty(fhrPartSet2)
    iIndices=[]; jIndices=[];
    return
end
concatSet = add(fhrPartSet1, fhrPartSet2);
concatIntervals = toIntervals( concatSet, DO_MERGE);

[unionIntervals iIndices jIndices] = unique(concatIntervals);

% outputs

length1 = length(fhrPartSet1);
length2 = length(fhrPartSet2);

temp = getRawData(concatSet);
fhrPartSet1.set = temp(iIndices);

jIndices = iIndices;
iIndices(find(iIndices > length1)) = [];
jIndices(find(iIndices <= length1)) = [];
jIndices = jIndices - length1;
return
