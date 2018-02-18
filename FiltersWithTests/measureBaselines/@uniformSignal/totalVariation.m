%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/totalVariation
%
%   Description:    
%		tv filter signal with given width 
%
%   Parameters:
%		this		(signal)
%		width		the width of the tv filter
%
%   Returns:
%		derivSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function tvSignal = totalVariation(this, width)
thisSamples = get(this, 'samples');
nSamples = length(thisSamples);
halfWidth = floor(width/2);
preSamples = ones(1, halfWidth) * thisSamples(1);
postSamples = ones(1, halfWidth) * thisSamples(end);
thisSamples = [preSamples thisSamples postSamples];
tvSamples = zeros(1, nSamples);
for i = 1:nSamples
    window = thisSamples(i : i + 2 * halfWidth);
    tvSamples(i) = sum(window);
end
tvSignal = uniformSignal('', 1, length(tvSamples), tvSamples);
return;
