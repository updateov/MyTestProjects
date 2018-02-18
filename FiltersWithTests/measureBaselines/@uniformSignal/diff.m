%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/diff
%
%   Description:    
%		compute the difference signal signal1-signal2
%
%   Parameters:
%		signal1		(signal)
%		signal2		(signal)
%
%   Returns:
%		diffSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function diffSignal = diff(signal1, signal2)
signal1Samples = get(signal1, 'samples');
signal2Samples = get(signal2, 'samples');
nSamples = length(signal1Samples);
if (nSamples ~= length(signal2Samples))
    error('The number of samples in signal1 and signal2 should be the same');
end

% diff
diffSamples = signal1Samples - signal2Samples;
diffSignal = uniformSignal('', 1, length(diffSamples), diffSamples);
return;
