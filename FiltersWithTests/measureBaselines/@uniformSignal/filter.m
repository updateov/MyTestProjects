%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/filter
%
%   Description:    
% 		filters the signal fhr_in using filter structure
% 		that discribes a FIR filter coefficients as Direct Form II Transposed"
% 
%     a(1)*y(n) = b(1)*x(n) + b(2)*x(n-1) + ... + b(nb+1)*x(n-nb)
%                          - a(2)*y(n-1) - ... - a(na+1)*y(n-na)
% 
%     If a(1) is not equal to 1, filt_del normalizes the filter
%     coefficients by a(1).
% 		Signal produced by this function is not delayed, t.e., the group delay
% 		coused by a filter is canceled.
%
%   Parameters:
% 		this					(uniformSignal)
%		fir_filt				a Matlab FIR filter
%		doCancelDelay	   (logical) enable group delay cancelling (default = T)
%		newName				name for output signal 
%
%   Returns:
%		outSignal			filtered signal (uniformSignal)
%
%	History:
%		21 Nov 2001			PAW: created 
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

function outSignal = filter(this, fir_filt, doCancelDelay, newName)

if nargin == 2
	newName = ['filtered ' this.signal.name];
	doCancelDelay = true
end

outSignal = uniformSignal(newName, get(this, 'Fs'), get(this, 'extent'), []); 

fhr_in = get(this.signal, 'samples');

fhr_row = size(fhr_in, 2);
fhr_col = size(fhr_in, 1);
fhr_len = fhr_row;
if fhr_col > fhr_row
  fhr_in  = fhr_in';
  fhr_len = fhr_col; 
end

if doCancelDelay
	%---{{{ Fill signal with zeros to cancel delays
	filt_del=floor(length(fir_filt.num)/2);
	if filt_del > 0
		fhr_in(fhr_len+filt_del)               = 0;
		fhr_in((fhr_len+1):(fhr_len+filt_del)) = zeros(1,(filt_del));
	end
	%---}}} Fill signal with zeros to cancel delays
	%---{{{ Filter input signal
	fhr_out = filter(fir_filt.num, fir_filt.den, fhr_in);
	%---}}} Filter input signal
	%---{{{ Remove steady-state
	if filt_del > 0
		fhr_out(1:(filt_del))=[];
	end
%---}}} Remove steady-state
else
	fhr_out = filter(fir_filt.num, fir_filt.den, fhr_in);
end
outSignal = set(outSignal, 'samples', fhr_out);
return
%---}}} function filt_del()
