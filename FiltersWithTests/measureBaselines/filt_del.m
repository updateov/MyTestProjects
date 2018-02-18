function fhr_out = filt_del(fhr_in, fir_filt, doPreviousMethod)
% Function filt_del() filters the signal fhr_in using filter structure
% that discribes a FIR filter coefficients as Direct Form II Transposed"
% 
%    a(1)*y(n) = b(1)*x(n) + b(2)*x(n-1) + ... + b(nb+1)*x(n-nb)
%                          - a(2)*y(n-1) - ... - a(na+1)*y(n-na)
% 
%    If a(1) is not equal to 1, filt_del normalizes the filter
%    coefficients by a(1).
% Signal produced by this function is not delayed, t.e., the group delay
% coused by a filter is canceled.

if nargin == 2
	doPreviousMethod = 0;
end
  
fhr_row = size(fhr_in, 2);
fhr_col = size(fhr_in, 1);
fhr_len = fhr_row;
if fhr_col > fhr_row
  fhr_in  = fhr_in';
  fhr_len = fhr_col; 
end

%---{{{ Fill signal with zeros to cancel delays
if doPreviousMethod == 1
	filterDelay=floor(size(fir_filt.num,2)/2);
else
	filterDelay=floor(length(fir_filt.num)/2);
end

if filterDelay > 0
  fhr_in(fhr_len+filterDelay)               = 0;
  fhr_in((fhr_len+1):(fhr_len+filterDelay)) = zeros(1,(filterDelay));
end
%---}}} Fill signal with zeros to cancel delays
%---{{{ Filter input signal
fhr_out=filter(fir_filt.num, fir_filt.den, fhr_in);
%---}}} Filter input signal
%---{{{ Remove stady-state
if filterDelay > 0
  fhr_out(1:(filterDelay))=[];
end
%---}}} Remove stady-state
return
