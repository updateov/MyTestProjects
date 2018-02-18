%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/signalToUniformSignal
%
%   Description:    
%		creates a uniformSignal from a signal at the original sampling rate
%		and extent.
%
%   Parameters:
%       inSignal			input (signal)
%
%   Returns:
%       aUniformSignal			output (uniformSignal)
%
%	History:
%		14 Aug 2001			PAW: adapted from MM's tend_create() function
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function aUniformSignal = signalToUniformSignal(inSignal)
samples = interp1(inSignal.indices,inSignal.samples,[ 1.1 [2:inSignal.extent] ] );  %-- just to make the interp1() function happy and prevent putting NaN
aUniformSignal = uniformSignal(inSignal.name, inSignal.Fs, inSignal.extent, samples);
return;
