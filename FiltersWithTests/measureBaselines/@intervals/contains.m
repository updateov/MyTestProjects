%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/contains
%
%   Description:    
%		finds an interval, if any, containing the given index
%
%   Parameters:
%		this    (intervals) 
%		index	(double) the index
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = contains(this, index)
indexLess = max(find(this.x1 <= index));
indexGreater = min(find(this.x2 >= index));
if indexLess == indexGreater
    index = indexLess;
else
    index = [];
end
this = subset(this, index);
return
