%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/derivative
%
%   Description:    
%		approximates the n-th order derivative of a signal
%
%   Parameters:
%		this		(signal)
%		order		the order of the derivative to calculate
%       alignment1   (boolean) true = align with first sample (default)
%                              false = align with second sample
%
%   Returns:
%		derivSignal		(signal)
%
%	History:
%		$Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function derivSignal = derivative(this, order, alignment1)
if nargin < 3
    alignment1 = true;
end
thisSamples = get(this, 'samples');
thisSignal = signal(thisSamples, 1:length(thisSamples));
derivSignal = derivative(thisSignal, order, alignment1);
derivSamples = get(derivSignal, 'samples');
derivSignal = uniformSignal('', 1, length(derivSamples), derivSamples);
return;
