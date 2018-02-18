%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/oneElementIntersectPercent
%
%   Description:    
%		determines the intervals in this that overlaps the 
%       specified interval using the specified overlap threshold 
%       (0 <=overlap <= 1) and also determine their degree of overlap
%       as a percentage
%
%   Parameters:
%		this            (intervals)
%       interval2       (intervals) - assumed to have one element only
%       overlapThresh   (double)
%		
%   Returns:
%     overlapPercent      (double)    for each of the n elements in 
%                                     overlapIntervals, the percentage
%                                     overlap with an interval in this
%     overlapIndices      (double)    n indices of this that overlap interval2
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [overlapPercent, overlapIndices] = ...
    oneElementIntersectPercent(this, interval2, overlapThresh, USE_BIGGER_INTERVAL_AS_DENOM)

overlapIntervals = empty(this);
overlapPercent = [];
overlapIndices = [];

% USE_BIGGER_INTERVAL_AS_DENOM is optional flag that can be set to use the
% bigger of the two intervals as the denom when calculating overlap perc. -
% default is that 'this' interval always used as denom - can be issue when
% interval2 is much larger.  Flag defaults to 0 for backwards compatibility
if (~(exist('USE_BIGGER_INTERVAL_AS_DENOM')))
    USE_BIGGER_INTERVAL_AS_DENOM = 0;
end

x1 = get(interval2, 'x1');
x2 = get(interval2, 'x2');
lenInt2 = x2 - x1 + 1;

%% the four intersect cases are NOT mutually exclusive 

% find intervals where interval2 inside
% 1 --------------
% 2   ---------- 
overlap = (x2-x1+1)./(this.x2-this.x1+1);
indices = find(x1 > this.x1 & ...
               x2 < this.x2 & ...
               overlap >= overlapThresh); 
if ~isempty(indices)
    overlapIndices = [overlapIndices indices];
    overlapPercent = [overlapPercent overlap(indices)];
end

% find intervals where interval2 outside (or same)
% 1   ---------- 
% 2 --------------
overlap = (this.x2-this.x1+1)./(x2-x1+1);
indices = find(x1 <= this.x1 & ...
               x2 >= this.x2 & ...
               overlap >= overlapThresh); 
if ~isempty(indices)
    overlapIndices = [overlapIndices indices];
    overlapPercent = [overlapPercent overlap(indices)];
end

% find intervals where interval2 lags
% 1 --------------
% 2    -----------
overlap = (this.x2-x1+1)./(this.x2-this.x1+1);
indices = find((x1 > this.x1) & ...
               (x1 <= this.x2) & ...    
               (x2 >= this.x2) & ...
               overlap >= overlapThresh); 
if ~isempty(indices)
    if USE_BIGGER_INTERVAL_AS_DENOM
        for i = 1:length(indices)
            overlapIndices = [overlapIndices indices(i)];
            len = this.x2(indices(i)) - this.x1(indices(i)) + 1;
            numerator = this.x2(indices(i)) - x1 + 1;
            denom = max(len, lenInt2);
            overlapPercent = [overlapPercent (numerator / denom)];
        end
    else
        overlapIndices = [overlapIndices indices];
        overlapPercent = [overlapPercent overlap(indices)];
    end
end

% find intervals where interval2 leads
% 1   ---------
% 2 -----------
overlap = (x2-this.x1+1)./(this.x2-this.x1+1);
indices = find(x1 <= this.x1 & ...
               x2 >= this.x1 & ...
               x2 <  this.x2 & ...
               overlap >= overlapThresh); 
if ~isempty(indices)
    if USE_BIGGER_INTERVAL_AS_DENOM
        for i = 1:length(indices)
            overlapIndices = [overlapIndices indices(i)];
            len = this.x2(indices(i)) - this.x1(indices(i)) + 1;
            numerator = x2 - this.x1(indices(i)) + 1;
            denom = max(len, lenInt2);
            overlapPercent = [overlapPercent (numerator / denom)];
        end
    else
        overlapIndices = [overlapIndices indices];
        overlapPercent = [overlapPercent overlap(indices)];
    end
end
return


