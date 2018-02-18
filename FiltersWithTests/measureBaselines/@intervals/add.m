%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/add
%
%   Description:    
%		adds the specified intervals to an intervals
%
%   Parameters:
%		this
%		interval2		(intervals) the intervals to add to this
%
%   Returns:
%       this			(intervals)
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = add(this, interval2)
this.x1 = [this.x1 get(interval2, 'x1')];
this.x2 = [this.x2 get(interval2, 'x2')];
return

