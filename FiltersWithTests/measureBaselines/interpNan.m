function uEst = interpNan(u, method, doExtrap)
if nargin < 3
    doExtrap = false;
end
if nargin < 2
    method = 'linear';
end
uValidI = find(~isnan(u));
ui = setdiff(1:length(u), uValidI);
if doExtrap
    uEstI = interp1(uValidI, u(uValidI), ui, method, 'extrap');
else    
    uEstI = interp1(uValidI, u(uValidI), ui, method);
end
uEst = u;
uEst(ui) = uEstI;
return