%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/removeEnclosed
%
%   Description:    
%       find any intervals that are enclosed by others and
%       remove them
%		
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
function this = removeEnclosed(this)
sort(this);
x1 = this.x1;
x2 = this.x2;
done = false;
while ~ done
    noEnclosed = true;
    for i = 1:length(x1)-1
        enclosedI = find(x1(i) <= x1(i+1:end) & x2(i) >= x2(i+1:end));
        x1(i+enclosedI) = [];
        x2(i+enclosedI) = [];
        if ~isempty(enclosedI)
            noEnclosed = false;
            break
        end
    end
    done = noEnclosed;
end
this.x1 = x1;
this.x2 = x2;
return

