%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/toIntervals
%
%   Description:    
%	   converts this to an intervals object
%
%   Signature
%    out = toIntervals(this)
%    out = toIntervals(this, doMerge)
%
%   Parameters:
%       this              (fhrPartSet)  
%       doMerge           (logical) sort and accept unique elements only
%   Returns:
%       out               (intervals)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = toIntervals(this, doMerge)
if nargin == 1
    doMerge = true;
end
out = intervals;
if doMerge
    for i = 1:length(this.set)
        out = merge(out, getInterval(this.set{i}));
    end
else
    for i = 1:length(this.set)
        out = add(out, getInterval(this.set{i}));
    end
end    
return

