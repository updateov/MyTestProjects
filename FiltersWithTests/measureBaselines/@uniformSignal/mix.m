%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/mix
%
%   Description:    
%		mix signal signal with another with the proportions controlled by 
%       a modulating function: 
%           out = (modFunction)*signal1 + (1 - modFunction)*signal2
%
%   Parameters:
%		signal1		(signal)
%		signal2		(signal)
%		modFunction	the modulating function should be in interval (0..1)
%                   and of same length as signal1
%
%   Returns:
%		derivSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function mixSignal = mix(signal1, signal2, modFunction)
signal1Samples = get(signal1, 'samples');
signal2Samples = get(signal2, 'samples');
nSamples = length(signal1Samples);
if (nSamples ~= length(modFunction)) & (nSamples ~= length(signal2Samples))
    error('The number of samples in signal1, signal2 and modFunction should be the same');
end

% equalize inputs
% gain = mean(signal1Samples./signal2Samples);
% signal2Samples = signal2Samples * gain;

% mix
mixSamples = modFunction.*signal1Samples + (1 - modFunction).*signal2Samples;
mixSignal = uniformSignal('', 1, length(mixSamples), mixSamples);

% figure;
% hold on;
% plot(signal1Samples, 'r-');
% plot(signal2Samples, 'r-');
% plot(modFunction + 100, 'k-');
% plot(mixSamples, 'b-');
% title('Mixed of signal1 and signal2');
% pause;
return;
