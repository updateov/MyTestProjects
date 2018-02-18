%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/toStringHMS
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
function str = toStringHMS(this)
str = '';
nElements = length(getX1(this));
for i = 1:nElements
    x1 = getX1(this, i);
    x2 = getX2(this, i);
    str = sprintf('%s%3d %s - %s %7d %7d %7d', ...
                    str, ...
                    i, ...
                    h_m_s_string(x1, '::'), ...
                    h_m_s_string(x2, '::'), ...
                    x1, ...
                    x2, ...
                    x2 - x1 + 1);
    if i < nElements
        str = sprintf('%s\n', str);
    end
end
return

