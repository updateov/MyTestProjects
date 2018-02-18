%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/extract
%
%   Description:    
%		extracts a signal from the input signal with the specified limits
%
%   Parameters:
%		this	(signal)
%		interval (intervals) the interval of signal to extract
%
%   Returns:
%     this	(signal)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = extract(this, interval)
thisSamples = get(this, 'samples');
limits = toVector(interval);
if limits(2) > length(thisSamples)
	limits(2) = length(thisSamples);
end
this = set(this, 'samples', thisSamples(limits(1):limits(2)));
return
