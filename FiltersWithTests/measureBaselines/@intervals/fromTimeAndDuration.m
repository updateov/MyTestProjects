%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/fromTimeAndDuration
%
%   Description:    
%		creates intervals by converting to relative samples from date 
%       strings (with a reference date) and durations 
%
%   Parameters:
%		this  (intervals)
%       referenceTimeStr (char) referenceTime (sample 1)
%       eventTimes (cell) cell of char event times
%       duration (double) event duration in seconds
%       Ts (double) sampling time
%
%   Returns:
%       flag  (boolean)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = fromTimeAndDuration(this, referenceTimeStr, eventTimes, duration, Ts)
dataFormatStr = 'yyyy-mm-dd HH:MM:SS';
refTime = datenum(referenceTimeStr, dataFormatStr);
nEvents = length(duration);
this.x1 = nan(1, nEvents);
this.x2 = nan(1, nEvents);

for iEvent = 1:nEvents
    this.x1(iEvent) = (datenum(eventTimes{iEvent}, dataFormatStr)-refTime)*24*3600/Ts + 1;
    this.x2(iEvent) = this.x1(iEvent)+(duration(iEvent)/Ts)-1;
end
return;
