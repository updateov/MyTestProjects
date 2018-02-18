%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/oneElementIntersect
%
%   Description:    
%		determines the intervals in this that overlaps the 
%       specified interval using the specified overlap threshold 
%       (0 <=overlap <= 1)
%
%   Parameters:
%		this  (intervals)
%       interval2  (intervals) - assumed to have one element only
%       overlap (double)
%		
%   Returns:
%     overlapIntervals   (intervals)    intervals in this that overlap
%                                       interval2
%     nonOverlapIntervals (intervals)   intervals in this that overlap
%                                       interval2
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [overlapIntervals, nonOverlapIntervals] = oneElementIntersect(this, interval2, overlap, USE_BIGGER_INTERVAL_AS_DENOM)
%overlapIntervals = intervals;
overlapIntervals = empty(this);
%disp('oneElementIntersect(): this');
%this
%disp('oneElementIntersect(): interval2');
%interval2

% USE_BIGGER_INTERVAL_AS_DENOM is optional flag that can be set to use the
% bigger of the two intervals as the denom when calculating overlap perc. -
% default is that 'this' interval always used as denom - can be issue when
% interval2 is much larger.  Flag defaults to 0 for backwards compatibility
if (~(exist('USE_BIGGER_INTERVAL_AS_DENOM')))
    USE_BIGGER_INTERVAL_AS_DENOM = 0;
end

x1 = get(interval2, 'x1');
x2 = get(interval2, 'x2');

%% the four intersect cases are NOT mutually exclusive 

% find intervals where interval2 inside
% 1 --------------
% 2   ---------- 
indices = find(x1 > this.x1 & ...
               x2 < this.x2 & ...
               (x2-x1+1)./(this.x2-this.x1+1) >= overlap); 
%overlapIntervals = subset(this, indices);   
allIndices = indices;

% find intervals where interval2 outside (or same)
% 1   ---------- 
% 2 --------------
indices = find(x1 <= this.x1 & ...
               x2 >= this.x2 & ...
               (this.x2-this.x1+1)./(x2-x1+1) >= overlap); 
%overlapIntervals = add(overlapIntervals, subset(this, indices));               
allIndices = [allIndices indices];

% find intervals where interval2 lags
% 1 --------------
% 2    -----------
if USE_BIGGER_INTERVAL_AS_DENOM
    indices = find((x1 > this.x1) & ...
               (x1 <= this.x2) & ...    
               (x2 >= this.x2) & ...
               (((this.x2-x1+1)./(this.x2-this.x1+1)) >= overlap) & ...
               (((this.x2-x1+1)./(x2-x1+1)) >= overlap));
else
    indices = find((x1 > this.x1) & ...
               (x1 <= this.x2) & ...    
               (x2 >= this.x2) & ...
               (((this.x2-x1+1)./(this.x2-this.x1+1)) >= overlap)); 
%overlapIntervals = add(overlapIntervals, subset(this, indices));   
end

allIndices = [allIndices indices];

% find intervals where interval2 leads
% 1   ---------
% 2 -----------
if USE_BIGGER_INTERVAL_AS_DENOM
    indices = find(x1 <= this.x1 & ...
               x2 >= this.x1 & ...
               x2 <  this.x2 & ...
               ((x2-this.x1+1)./(this.x2-this.x1+1) >= overlap) & ...
               (((x2 - this.x1 + 1)./(x2-x1+1)) >= overlap)); 
else
    indices = find(x1 <= this.x1 & ...
               x2 >= this.x1 & ...
               x2 <  this.x2 & ...
               (x2-this.x1+1)./(this.x2-this.x1+1) >= overlap); 
end
allIndices = [allIndices indices];
%overlapIntervals = add(overlapIntervals, subset(this, indices));              
overlapIntervals = add(overlapIntervals, subset(this, allIndices));              
               
%nonOverlapIndices = setDiff(1:size(this), indices);   
nonOverlapIndices = setdiff(1:size(this), allIndices);   
nonOverlapIntervals = subset(this, nonOverlapIndices);   
return


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/oneElementIntersect
%
%   Description:    
%		determines the intervals in this that overlaps the 
%       specified interval using the specified overlap threshold 
%       (0 <=overlap <= 1)
%
%   Parameters:
%		this  (intervals)
%       interval2  (intervals) - assumed to have one element only
%       overlap (double)
%
%       Note: this is equivalent to the above, but may run faster
%       (but less easy to debug).
%		
%   Returns:
%     overlapIntervals   (intervals)    intervals in this that overlap
%                                       interval2
%     nonOverlapIntervals (intervals)   intervals in this that overlap
%                                       interval2
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

function [overlapIntervals, nonOverlapIntervals] = oneElementIntersectV4(this, interval2, overlap)
overlapIntervals = intervals;
%disp('oneElementIntersect(): this');
%this
%disp('oneElementIntersect(): interval2');
%interval2
x1 = get(interval2, 'x1');
x2 = get(interval2, 'x2');

% the four intersect cases are mutually exclusive

% Case 1
% interval2 inside this
% 1 --------------
% 2   ---------- 
% Case 2
% interval2 outside (or same as) this
% 1   ---------- 
% 2 --------------
% Case 3
% interval2 lags this
% 1 --------------
% 2    -----------
% Case 4
% interval2 leads this
% 1   ---------
% 2 -----------
doCheck = find((this.x2-this.x1+1) == 0 | ...
               (x2-x1+1) == 0);
if ~isempty(doCheck)
    gotHere = 1;
end
indices = find(... 
               (x1 > this.x1 & ... % Case 1
                x2 < this.x2 & ...
                (x2-x1+1)./(this.x2-this.x1+1) >= overlap) | ...
               (x1 <= this.x1 & ...% Case 2 
                x2 >= this.x2 & ...
                (this.x2-this.x1+1)./(x2-x1+1) >= overlap) | ...
               (x1 >  this.x1 & ...% Case 3
                x1 <= this.x2 & ...    
                x2 >= this.x2 & ...
                (this.x2-x1+1)./(this.x2-this.x1+1) >= overlap) | ...
               (x1 <= this.x1 & ...% Case 4
                x2 >= this.x1 & ...
                x2 <  this.x2 & ...
                (x2-this.x1+1)./(this.x2-this.x1+1) >= overlap)); 

overlapIntervals = subset(this, indices);   
nonOverlapIndices = setDiff(1:size(this), indices);   
nonOverlapIntervals = subset(this, nonOverlapIndices);   
return

