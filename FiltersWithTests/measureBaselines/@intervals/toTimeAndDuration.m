%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/toTimeAndDuration
%
%   Description:    
%		convert to a date-time string and duration format
%
%   Parameters:
%		this  (intervals)
%		
%   Returns:
%     time  (cell) of interval begin date-time string
%     duration (double) durations in seconds
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [time, duration] = toTimeAndDuration(this, startTime, Ts)
dataFormatStr = 'yyyy-mm-dd HH:MM:SS';

x1 = getX1(this);
x2 = getX2(this);
time = cell(size(x1));
duration = nan(size(x1));
for iInterval = 1:length(x1)
    time{iInterval} = datestr(startTime + ...
                          (x1(iInterval)-1)*Ts/(3600*24), dataFormatStr);
    duration(iInterval) = (x2(iInterval) - x1(iInterval)+1) * Ts;
end
return