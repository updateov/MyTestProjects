%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/modulatedAverage
%
%   Description:    
%		average the signal using a window width proportional to the maxWidth
%       times the modulating function
%
%   Parameters:
%		this		(signal)
%		minWidth	the minimum width of the average filter
%		maxWidth	the maximum width of the average filter
%		modFunction	the modulating function should be in interval (0..1)
%                   and of same length as this
%
%   Returns:
%		derivSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function averageSignal = modulatedAverage(this, minWidth, maxWidth, modFunction)
thisSamples = get(this, 'samples');
nSamples = length(thisSamples);
if nSamples ~= length(modFunction)
    error('The number of samples in this and modFunction should be the same');
end
halfMaxWidth = floor(maxWidth/2);
preSamples = ones(1, halfMaxWidth) * thisSamples(1);
postSamples = ones(1, halfMaxWidth) * thisSamples(end);
thisSamples = [preSamples thisSamples postSamples];
averageSamples = zeros(1, nSamples);
halfWidth = floor((minWidth + ((maxWidth-minWidth+1) * modFunction)) / 2);
for i = 1:nSamples
    window = thisSamples(i + halfMaxWidth - halfWidth(i): i + halfMaxWidth + halfWidth(i));
    averageSamples(i) = mean(window);
end
averageSignal = uniformSignal('', 1, length(averageSamples), averageSamples);
% figure;
% hold on;
% plot(thisSamples(halfMaxWidth+1:end-halfMaxWidth), 'r-');
% plot(averageSamples - 0.03, 'b-');
% plot(halfWidth*2, 'b-');
% title('Average filtering: Input-red Output-blue');
% pause;
return;
