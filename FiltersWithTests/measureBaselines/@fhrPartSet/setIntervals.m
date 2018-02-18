%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/setIntervals
%
%   Description:    
%
%	 Parameters:
%       this              (fhrPartSet)  
%       anIntervals       (intervals)  of length == length(this)
%
%   Returns:
%       this              (fhrPartSet)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = setIntervals(this, anIntervals)
if length(this) ~= size(anIntervals)
    error('The number of intervals must match the number of parts');
end
for i = 1:length(this)
    curPart = this.set{i};
    curPart = setInterval(curPart, subset(anIntervals, i));
    this.set{i} = curPart;
end
return

