%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/toSignal
%
%   Description:    
%		resamples a uniformSignal at the given sampling indices
%
%   Parameters:
% 		this				(uniformSignal)
%		indices				(double) array of indices into signal where 
%									 sampling should occur
%		doResample			(logical) if false, just return the samples at the 
%									  given indices in a new signal object.
%
%   Returns:
%		outSignal			newly sampled array (signal)
%
%	History:
%		14 Aug 2001			PAW: adapted from MM's tend_create() function
%		21 Sept 2001		PAW: created as class
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function outSignal = toSignal(this, indices, doResample)

outSignal = signal(get(this.signal, 'name'), get(this.signal, 'Fs'), get(this.signal, 'extent'), [], indices); 

if ~doResample
	inSamples = get(this.signal, 'samples');
	outSignal = set(outSignal, 'samples', inSamples(indices));
	outSignal = set(outSignal, 'indices', indices);
	return
end
	
nSamples = length(indices);

%---{{{ Calculate at first sample
len = indices(2)-indices(1) + 1;
RIGHT_weight_vector = sin([1:len]*(pi/(len-1)) - (pi/(len-1)) + (pi/2)) + 1;
RIGHT_weight_vector = RIGHT_weight_vector / sum(RIGHT_weight_vector);
outSignal.samples(1) = sum(this.signal.samples(indices(1):indices(2)).*RIGHT_weight_vector);
%---}}} Calculate at first sample
%---{{{ Calculate between the first and the last sample
for index=2:(nSamples - 1)
   len = indices(index)-indices(index-1) + 1;
   LEFT_weight_vector = sin([1:len]*(pi/(len-1)) - (pi/(len-1)) - (pi/2)) + 1;
   LEFT_weight_vector = LEFT_weight_vector / ( sum(LEFT_weight_vector) * 2 );
   len = indices(index+1)-indices(index) + 1;
   RIGHT_weight_vector = sin([1:len]*(pi/(len-1)) - (pi/(len-1)) + (pi/2)) + 1;
   RIGHT_weight_vector = RIGHT_weight_vector / ( sum(RIGHT_weight_vector) * 2 );
   
   
   %---{{{ Calculate the result as sum of the left and right weight   
   outSignal.samples(index)= sum(this.signal.samples(indices(index-1):indices(index)).*LEFT_weight_vector) + ...
                    sum(this.signal.samples(indices(index):indices(index+1)).*RIGHT_weight_vector);
   %---}}} Calculate the result as sum of the left and right weight
end
%---}}} Calculate between the first and the last sample
%---{{{ Calculate at last sample
len = indices(nSamples)-indices(nSamples-1) + 1;
LEFT_weight_vector = sin([1:len]*(pi/(len-1)) - (pi/(len-1)) - (pi/2)) + 1;
LEFT_weight_vector = LEFT_weight_vector / sum(LEFT_weight_vector);
outSignal.samples(nSamples)= sum(this.signal.samples(indices(nSamples-1):indices(nSamples)).*LEFT_weight_vector);
%---}}} Calculate at last sample

return;
   
