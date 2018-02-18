%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/median
%
%   Description:    
%		median filter signal with given width 
%
%   Parameters:
%		this		(signal)
%		width		the width of the median filter
%
%   Returns:
%		derivSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function medianSignal = median(this, width, extrapMethod)
if nargin<3
    extrapMethod = 'endSample'
end
thisSamples = get(this, 'samples');
nSamples = length(thisSamples);
halfWidth = floor(width/2);

switch extrapMethod
    case 'endSample'
        preSamples = ones(1, halfWidth) * thisSamples(1);
        postSamples = ones(1, halfWidth) * thisSamples(end);
    case 'mirror'
        preSamples = fliplr(thisSamples(1:halfWidth));
        postSamples = fliplr(thisSamples((end-halfWidth+1):end));        
end
thisSamples = [preSamples thisSamples postSamples];
medianSamples = zeros(1, nSamples);
for i = 1:nSamples
    window = thisSamples(i : i + 2 * halfWidth);
    medianSamples(i) = nanmedian(window);
end
medianSignal = uniformSignal('', 1, length(medianSamples), medianSamples);
% figure;
% hold on;
% % plot(thisSamples(halfWidth+1:end-halfWidth), 'r-');
% plot(medianSamples - 0.03, 'b-');
% title('Median filtering: Input-red Output-blue');
% pause;
return;
