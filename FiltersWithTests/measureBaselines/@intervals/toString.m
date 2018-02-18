%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/toString
%
%   Description:    
%		string representation of an interval
%
%   Parameters:
%		this 				(interval)
%
%   Returns:
%     none
%
%	History:
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function str = toString(this)
str = sprintf('name: %s\n', this.name);
str = sprintf('%snumber of intervals: %d\n', str, length(this.x1));
iStr = sprintf('%7d ', 1:length(this.x1));
x1Str = sprintf('%7d ', this.x1);
x2Str = sprintf('%7d ', this.x2);
str = sprintf('%s%3s  %s\nx1:  %s\nx2:  %s\n', str, 'i:', iStr, x1Str, x2Str);
return

