%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/findClosestPostSampleGreaterEqual
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
function outIndex = findClosestPostSampleGreaterEqual(this, index, sampleValue)
thisSamples = get(this, 'samples');
searchSamples = thisSamples(index+1:end);
matchI = min(find(searchSamples >= sampleValue));
outIndex = index + matchI;
return
