%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/concat
%
%   Description:    
%		concatenates the incoming signals into an output signal.  The indices
%		of the output signal are correct (i.e. the second signal indices are
%		shifted)
%
%   Parameters:
%		this  	(signal)
%		signal2	(signal)
%
%   Returns:
%     this		(signal)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function outSignal = concat(this, signal2)
outSamples = [get(this, 'samples') get(signal2,'samples')];
outSignal = uniformSignal('concat', get(this, 'Fs'), length(outSamples), outSamples); 
return

