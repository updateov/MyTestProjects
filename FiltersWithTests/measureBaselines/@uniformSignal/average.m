%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/average
%
%   Description:    
%		average filter signal with given width 
%
%   Parameters:
%		this		(signal)
%		width		the width of the average filter
%
%   Returns:
%		derivSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function averageSignal = average(this, width)
thisSamples = get(this, 'samples');
nSamples = length(thisSamples);
halfWidth = floor(width/2);
preSamples = ones(1, halfWidth) * thisSamples(1);
postSamples = ones(1, halfWidth) * thisSamples(end);
thisSamples = [preSamples thisSamples postSamples];
averageSamples = zeros(1, nSamples);
for i = 1:nSamples
    window = thisSamples(i : i + 2 * halfWidth);
    averageSamples(i) = mean(window);
end
averageSignal = uniformSignal('', 1, length(averageSamples), averageSamples);
% figure;
% hold on;
% plot(thisSamples(halfWidth+1:end-halfWidth), 'r-');
% plot(averageSamples - 0.03, 'b-');
% title('Average filtering: Input-red Output-blue');
% pause;
return;
