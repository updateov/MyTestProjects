%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/subset
%
%   Description:    
%		extracts a signal from the input signal from the specified indices
%
%   Parameters:
%		this	(signal)
%		indices (double) the desired signal indices
%
%   Returns:
%     this	(signal)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function out = subset(this, indices)
thisSamples = get(this, 'samples');
out = signal(thisSamples(indices), indices);
return
