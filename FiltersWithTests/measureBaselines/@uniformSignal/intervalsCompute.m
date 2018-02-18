%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/intervalsCompute
%
%   Description:    
%		compute the given numeric function on the samples over the 
%       given intervals
%
%   Parameters:
%		this		(uniformSignal)
%       theIntervals   (intervals)
%       fn          (function handle)
%   
%
%   Returns:
%		derivSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = intervalsCompute(this, theIntervals, fn);
thisSamples = get(this, 'samples');

x1 = getX1(theIntervals);
x2 = getX2(theIntervals);

for i = 1:size(theIntervals)
    currInterval = subset(theIntervals, i);
    currSamples = thisSamples(x1(i):x2(i));
    out(i) = feval(fn, currSamples);
end

return;
