%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/plot
%
%   Description:    
%		plot the fhrPart at the specified level and colors
%
%	 Parameters
%       this (intervals)
%       level (double) - y value of plotted line
%       color (3-element double)
%
%
%   Returns:
%     h		handle to plotted line
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function plot(this, level, color)
if nargin == 1
	color = [0 0 1];
    level = 100;
end
for i = 1:length(this.set)
    plot(this.set{i}, level, color);
end
return
