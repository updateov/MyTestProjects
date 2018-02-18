%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/addSample
%
%   Description:    
%		adds a sample to the end of the samples vector
%
%   Parameters:
%		this				(uniformSignal)
%		index				the index of the sample 
%		value				the value of the sample
%
%   Returns:
%       this			output (signal)
%
%	History:
%		14 Aug 2001			PAW
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = addSample(this, value)

lastIndexSamples = length(this.samples);
lastIndexIndices = length(this.indices);
if (lastIndexSamples ~= lastIndexIndices)
	error('The arrays samples and indices must be of the same length');
end

this.samples(lastIndexSamples + 1) = value;
this.indices(lastIndexIndices + 1) = lastIndexIndices + 1;

return;
