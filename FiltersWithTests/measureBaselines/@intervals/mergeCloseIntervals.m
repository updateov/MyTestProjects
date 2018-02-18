%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/mergeCloseIntervals
%
%   Description:    
%		string representation of an interval
%
%   Parameters:
%		this 				(interval)
%       gap                 (double) successive intervals with gaps of 
%                           length greater than this value will be merged
%
%   Returns:
%     none
%
%	History:
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = mergeCloseIntervals(this, gap)
gaps = this.x1(2:end) - this.x2(1:end-1);
shortGaps = find(gaps < gap);

% figure;
% plot(1,1); hold on;
% plot(this, 50, 'r', true);

this.x1(shortGaps+1) = [];
this.x2(shortGaps) = [];

% plot(this, 100, 'k', true);

return

