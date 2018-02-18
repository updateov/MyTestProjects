%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/findClosestPrevSampleGreaterEqual
%
%   Description:    
%		finds the closest previous sample whose value is greater less than 
%       or equal to the specified value
%
%   Parameters:
%		this    (uniformSignal) 
%		index	(double) the index before which to search
%		sampleValue	(double) the sample threshold
%
%   Returns:
%       outIndex (double) the corresponding index
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function outIndex = findClosestPrevSampleGreaterEqual(this, index, sampleValue)
thisSamples = get(this, 'samples');
searchSamples = thisSamples(1:index-1);
outIndex = max(find(searchSamples >= sampleValue));
return
