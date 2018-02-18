%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/shift
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
function this = shift(this, xTransl)
for i = 1:length(this)
    curPart = this.set{i};
    curPart = shift(curPart, xTransl);
    this.set{i} = curPart;
end
return

