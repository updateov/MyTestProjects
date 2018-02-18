%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/plot
%
%   Description:    
%		plot the intervals at the specified level and colors
%
%	 Parameters
%       this (intervals)
%       level (double) - y value of plotted line
%       color (3-element double)
%		doVerticalBars (logical)
%		yRange	(2-element double) limits of vertical bars
%
%   Returns:
%     h		handle to plotted line
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function plot(this, level, color, doVertBars, yRange, lineStyle, lineWidth, markerSize)
if nargin == 1
	color = [0 0 1];
    doVertBars = false;
    level = 80;
end
if nargin < 4
    doVertBars = false;
end
if nargin < 5
    yRange = [0 240];
end
if nargin < 6
    lineStyle = '-.' ;
end
if nargin < 7
    lineWidth = 1 ;
end
if nargin < 8
    markerSize = 1 ;
end

if (length(level) < size(this))
    level = ones(1, size(this)) * level(1);
end
for i=1:size(this)
    points = lmsPoints(getX1(this, i), level(i));
    points = merge(points, lmsPoints(getX2(this, i), level(i)));
    plotLine = lmsLine(points);
    h = plot(plotLine, color); hold on;
    set(h, 'Marker', 'o', 'MarkerSize', markerSize, 'LineWidth', lineWidth);
    if doVertBars
        plot([getX1(this,i) getX1(this,i)], yRange, lineStyle, 'LineWidth', lineWidth, 'Color', color); 
        plot([getX2(this,i) getX2(this,i)], yRange, lineStyle, 'LineWidth', lineWidth, 'Color', color); 
    end    
end
return

