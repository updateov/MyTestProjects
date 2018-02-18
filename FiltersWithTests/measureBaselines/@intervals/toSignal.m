%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/toSignal
%
%   Description:    
%		samples a signal over the intervals
%
%   Parameters:
%		this  (intervals)
%		
%   Returns:
%     vec     (2-element vector)  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function outSignal = toSignal(this, inSignal)
samples = [];
indices = [];
for i = 1:length(this.x1)
    samples = [samples inSignal(this.x1(i):this.x2(i))];
    indices = [indices this.x1(i):this.x2(i)];
end
outSignal = signal(samples, indices);
return